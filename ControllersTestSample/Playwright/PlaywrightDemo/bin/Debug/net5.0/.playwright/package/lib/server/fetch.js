"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.GlobalFetchRequest = exports.BrowserContextFetchRequest = exports.FetchRequest = void 0;

var http = _interopRequireWildcard(require("http"));

var https = _interopRequireWildcard(require("https"));

var _httpsProxyAgent = require("https-proxy-agent");

var _socksProxyAgent = require("socks-proxy-agent");

var _stream = require("stream");

var _url = _interopRequireDefault(require("url"));

var _zlib = _interopRequireDefault(require("zlib"));

var _debugLogger = require("../utils/debugLogger");

var _timeoutSettings = require("../utils/timeoutSettings");

var _utils = require("../utils/utils");

var _browserContext = require("./browserContext");

var _cookieStore = require("./cookieStore");

var _formData = require("./formData");

var _instrumentation = require("./instrumentation");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function _getRequireWildcardCache(nodeInterop) { if (typeof WeakMap !== "function") return null; var cacheBabelInterop = new WeakMap(); var cacheNodeInterop = new WeakMap(); return (_getRequireWildcardCache = function (nodeInterop) { return nodeInterop ? cacheNodeInterop : cacheBabelInterop; })(nodeInterop); }

function _interopRequireWildcard(obj, nodeInterop) { if (!nodeInterop && obj && obj.__esModule) { return obj; } if (obj === null || typeof obj !== "object" && typeof obj !== "function") { return { default: obj }; } var cache = _getRequireWildcardCache(nodeInterop); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj.default = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

/**
 * Copyright (c) Microsoft Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
class FetchRequest extends _instrumentation.SdkObject {
  static findResponseBody(guid) {
    for (const request of FetchRequest.allInstances) {
      const body = request.fetchResponses.get(guid);
      if (body) return body;
    }

    return undefined;
  }

  constructor(parent) {
    super(parent, 'fetchRequest');
    this.fetchResponses = new Map();
    FetchRequest.allInstances.add(this);
  }

  _disposeImpl() {
    FetchRequest.allInstances.delete(this);
    this.fetchResponses.clear();
    this.emit(FetchRequest.Events.Dispose);
  }

  _storeResponseBody(body) {
    const uid = (0, _utils.createGuid)();
    this.fetchResponses.set(uid, body);
    return uid;
  }

  async fetch(params) {
    try {
      var _params$method;

      const headers = {};

      const defaults = this._defaultOptions();

      headers['user-agent'] = defaults.userAgent;
      headers['accept'] = '*/*';
      headers['accept-encoding'] = 'gzip,deflate,br';

      if (defaults.extraHTTPHeaders) {
        for (const {
          name,
          value
        } of defaults.extraHTTPHeaders) headers[name.toLowerCase()] = value;
      }

      if (params.headers) {
        for (const {
          name,
          value
        } of params.headers) headers[name.toLowerCase()] = value;
      }

      const method = ((_params$method = params.method) === null || _params$method === void 0 ? void 0 : _params$method.toUpperCase()) || 'GET';
      const proxy = defaults.proxy;
      let agent;

      if (proxy) {
        var _proxyOpts$protocol;

        // TODO: support bypass proxy
        const proxyOpts = _url.default.parse(proxy.server);

        if ((_proxyOpts$protocol = proxyOpts.protocol) !== null && _proxyOpts$protocol !== void 0 && _proxyOpts$protocol.startsWith('socks')) {
          agent = new _socksProxyAgent.SocksProxyAgent({
            host: proxyOpts.hostname,
            port: proxyOpts.port || undefined
          });
        } else {
          if (proxy.username) proxyOpts.auth = `${proxy.username}:${proxy.password || ''}`;
          agent = new _httpsProxyAgent.HttpsProxyAgent(proxyOpts);
        }
      }

      const timeout = defaults.timeoutSettings.timeout(params);
      const deadline = timeout && (0, _utils.monotonicTime)() + timeout;
      const options = {
        method,
        headers,
        agent,
        maxRedirects: 20,
        timeout,
        deadline
      }; // rejectUnauthorized = undefined is treated as true in node 12.

      if (params.ignoreHTTPSErrors || defaults.ignoreHTTPSErrors) options.rejectUnauthorized = false;
      const requestUrl = new URL(params.url, defaults.baseURL);

      if (params.params) {
        for (const {
          name,
          value
        } of params.params) requestUrl.searchParams.set(name, value);
      }

      let postData;
      if (['POST', 'PUT', 'PATCH'].includes(method)) postData = serializePostData(params, headers);else if (params.postData || params.jsonData || params.formData || params.multipartData) throw new Error(`Method ${method} does not accept post data`);
      if (postData) headers['content-length'] = String(postData.byteLength);
      const fetchResponse = await this._sendRequest(requestUrl, options, postData);

      const fetchUid = this._storeResponseBody(fetchResponse.body);

      if (params.failOnStatusCode && (fetchResponse.status < 200 || fetchResponse.status >= 400)) return {
        error: `${fetchResponse.status} ${fetchResponse.statusText}`
      };
      return {
        fetchResponse: { ...fetchResponse,
          fetchUid
        }
      };
    } catch (e) {
      return {
        error: String(e)
      };
    }
  }

  async _updateCookiesFromHeader(responseUrl, setCookie) {
    const url = new URL(responseUrl); // https://datatracker.ietf.org/doc/html/rfc6265#section-5.1.4

    const defaultPath = '/' + url.pathname.substr(1).split('/').slice(0, -1).join('/');
    const cookies = [];

    for (const header of setCookie) {
      // Decode cookie value?
      const cookie = parseCookie(header);
      if (!cookie) continue; // https://datatracker.ietf.org/doc/html/rfc6265#section-5.2.3

      if (!cookie.domain) cookie.domain = url.hostname;else (0, _utils.assert)(cookie.domain.startsWith('.'));
      if (!(0, _cookieStore.domainMatches)(url.hostname, cookie.domain)) continue; // https://datatracker.ietf.org/doc/html/rfc6265#section-5.2.4

      if (!cookie.path || !cookie.path.startsWith('/')) cookie.path = defaultPath;
      cookies.push(cookie);
    }

    if (cookies.length) await this._addCookies(cookies);
  }

  async _updateRequestCookieHeader(url, options) {
    if (options.headers['cookie'] !== undefined) return;
    const cookies = await this._cookies(url);

    if (cookies.length) {
      const valueArray = cookies.map(c => `${c.name}=${c.value}`);
      options.headers['cookie'] = valueArray.join('; ');
    }
  }

  async _sendRequest(url, options, postData) {
    await this._updateRequestCookieHeader(url, options);
    return new Promise((fulfill, reject) => {
      const requestConstructor = (url.protocol === 'https:' ? https : http).request;
      const request = requestConstructor(url, options, async response => {
        if (_debugLogger.debugLogger.isEnabled('api')) {
          _debugLogger.debugLogger.log('api', `← ${response.statusCode} ${response.statusMessage}`);

          for (const [name, value] of Object.entries(response.headers)) _debugLogger.debugLogger.log('api', `  ${name}: ${value}`);
        }

        if (response.headers['set-cookie']) await this._updateCookiesFromHeader(response.url || url.toString(), response.headers['set-cookie']);

        if (redirectStatus.includes(response.statusCode)) {
          if (!options.maxRedirects) {
            reject(new Error('Max redirect count exceeded'));
            request.destroy();
            return;
          }

          const headers = { ...options.headers
          };
          delete headers[`cookie`]; // HTTP-redirect fetch step 13 (https://fetch.spec.whatwg.org/#http-redirect-fetch)

          const status = response.statusCode;
          let method = options.method;

          if ((status === 301 || status === 302) && method === 'POST' || status === 303 && !['GET', 'HEAD'].includes(method)) {
            method = 'GET';
            postData = undefined;
            delete headers[`content-encoding`];
            delete headers[`content-language`];
            delete headers[`content-length`];
            delete headers[`content-location`];
            delete headers[`content-type`];
          }

          const redirectOptions = {
            method,
            headers,
            agent: options.agent,
            maxRedirects: options.maxRedirects - 1,
            timeout: options.timeout,
            deadline: options.deadline
          }; // rejectUnauthorized = undefined is treated as true in node 12.

          if (options.rejectUnauthorized === false) redirectOptions.rejectUnauthorized = false; // HTTP-redirect fetch step 4: If locationURL is null, then return response.

          if (response.headers.location) {
            const locationURL = new URL(response.headers.location, url);
            fulfill(this._sendRequest(locationURL, redirectOptions, postData));
            request.destroy();
            return;
          }
        }

        if (response.statusCode === 401 && !options.headers['authorization']) {
          const auth = response.headers['www-authenticate'];

          const credentials = this._defaultOptions().httpCredentials;

          if (auth !== null && auth !== void 0 && auth.trim().startsWith('Basic ') && credentials) {
            const {
              username,
              password
            } = credentials;
            const encoded = Buffer.from(`${username || ''}:${password || ''}`).toString('base64');
            options.headers['authorization'] = `Basic ${encoded}`;
            fulfill(this._sendRequest(url, options, postData));
            request.destroy();
            return;
          }
        }

        response.on('aborted', () => reject(new Error('aborted')));
        let body = response;
        let transform;
        const encoding = response.headers['content-encoding'];

        if (encoding === 'gzip' || encoding === 'x-gzip') {
          transform = _zlib.default.createGunzip({
            flush: _zlib.default.constants.Z_SYNC_FLUSH,
            finishFlush: _zlib.default.constants.Z_SYNC_FLUSH
          });
        } else if (encoding === 'br') {
          transform = _zlib.default.createBrotliDecompress();
        } else if (encoding === 'deflate') {
          transform = _zlib.default.createInflate();
        }

        if (transform) {
          body = (0, _stream.pipeline)(response, transform, e => {
            if (e) reject(new Error(`failed to decompress '${encoding}' encoding: ${e}`));
          });
        }

        const chunks = [];
        body.on('data', chunk => chunks.push(chunk));
        body.on('end', () => {
          const body = Buffer.concat(chunks);
          fulfill({
            url: response.url || url.toString(),
            status: response.statusCode || 0,
            statusText: response.statusMessage || '',
            headers: toHeadersArray(response.rawHeaders),
            body
          });
        });
        body.on('error', reject);
      });
      request.on('error', reject);

      const disposeListener = () => {
        reject(new Error('Request context disposed.'));
        request.destroy();
      };

      this.on(FetchRequest.Events.Dispose, disposeListener);
      request.on('close', () => this.off(FetchRequest.Events.Dispose, disposeListener));

      if (_debugLogger.debugLogger.isEnabled('api')) {
        _debugLogger.debugLogger.log('api', `→ ${options.method} ${url.toString()}`);

        if (options.headers) {
          for (const [name, value] of Object.entries(options.headers)) _debugLogger.debugLogger.log('api', `  ${name}: ${value}`);
        }
      }

      if (options.deadline) {
        const rejectOnTimeout = () => {
          reject(new Error(`Request timed out after ${options.timeout}ms`));
          request.destroy();
        };

        const remaining = options.deadline - (0, _utils.monotonicTime)();

        if (remaining <= 0) {
          rejectOnTimeout();
          return;
        }

        request.setTimeout(remaining, rejectOnTimeout);
      }

      if (postData) request.write(postData);
      request.end();
    });
  }

}

exports.FetchRequest = FetchRequest;
FetchRequest.Events = {
  Dispose: 'dispose'
};
FetchRequest.allInstances = new Set();

class BrowserContextFetchRequest extends FetchRequest {
  constructor(context) {
    super(context);
    this._context = void 0;
    this._context = context;
    context.once(_browserContext.BrowserContext.Events.Close, () => this._disposeImpl());
  }

  dispose() {
    this.fetchResponses.clear();
  }

  _defaultOptions() {
    return {
      userAgent: this._context._options.userAgent || this._context._browser.userAgent(),
      extraHTTPHeaders: this._context._options.extraHTTPHeaders,
      httpCredentials: this._context._options.httpCredentials,
      proxy: this._context._options.proxy || this._context._browser.options.proxy,
      timeoutSettings: this._context._timeoutSettings,
      ignoreHTTPSErrors: this._context._options.ignoreHTTPSErrors,
      baseURL: this._context._options.baseURL
    };
  }

  async _addCookies(cookies) {
    await this._context.addCookies(cookies);
  }

  async _cookies(url) {
    return await this._context.cookies(url.toString());
  }

  async storageState() {
    return this._context.storageState();
  }

}

exports.BrowserContextFetchRequest = BrowserContextFetchRequest;

class GlobalFetchRequest extends FetchRequest {
  constructor(playwright, options) {
    super(playwright);
    this._cookieStore = new _cookieStore.CookieStore();
    this._options = void 0;
    this._origins = void 0;
    const timeoutSettings = new _timeoutSettings.TimeoutSettings();
    if (options.timeout !== undefined) timeoutSettings.setDefaultTimeout(options.timeout);
    const proxy = options.proxy;

    if (proxy !== null && proxy !== void 0 && proxy.server) {
      let url = proxy === null || proxy === void 0 ? void 0 : proxy.server.trim();
      if (!/^\w+:\/\//.test(url)) url = 'http://' + url;
      proxy.server = url;
    }

    if (options.storageState) {
      this._origins = options.storageState.origins;

      this._cookieStore.addCookies(options.storageState.cookies);
    }

    this._options = {
      baseURL: options.baseURL,
      userAgent: options.userAgent || `Playwright/${(0, _utils.getPlaywrightVersion)()}`,
      extraHTTPHeaders: options.extraHTTPHeaders,
      ignoreHTTPSErrors: !!options.ignoreHTTPSErrors,
      httpCredentials: options.httpCredentials,
      proxy,
      timeoutSettings
    };
  }

  dispose() {
    this._disposeImpl();
  }

  _defaultOptions() {
    return this._options;
  }

  async _addCookies(cookies) {
    this._cookieStore.addCookies(cookies);
  }

  async _cookies(url) {
    return this._cookieStore.cookies(url);
  }

  async storageState() {
    return {
      cookies: this._cookieStore.allCookies(),
      origins: this._origins || []
    };
  }

}

exports.GlobalFetchRequest = GlobalFetchRequest;

function toHeadersArray(rawHeaders) {
  const result = [];

  for (let i = 0; i < rawHeaders.length; i += 2) result.push({
    name: rawHeaders[i],
    value: rawHeaders[i + 1]
  });

  return result;
}

const redirectStatus = [301, 302, 303, 307, 308];

function parseCookie(header) {
  const pairs = header.split(';').filter(s => s.trim().length > 0).map(p => p.split('=').map(s => s.trim()));
  if (!pairs.length) return null;
  const [name, value] = pairs[0];
  const cookie = {
    name,
    value,
    domain: '',
    path: '',
    expires: -1,
    httpOnly: false,
    secure: false,
    sameSite: 'Lax' // None for non-chromium

  };

  for (let i = 1; i < pairs.length; i++) {
    const [name, value] = pairs[i];

    switch (name.toLowerCase()) {
      case 'expires':
        const expiresMs = +new Date(value);
        if (isFinite(expiresMs)) cookie.expires = expiresMs / 1000;
        break;

      case 'max-age':
        const maxAgeSec = parseInt(value, 10);
        if (isFinite(maxAgeSec)) cookie.expires = Date.now() / 1000 + maxAgeSec;
        break;

      case 'domain':
        cookie.domain = value.toLocaleLowerCase() || '';
        if (cookie.domain && !cookie.domain.startsWith('.')) cookie.domain = '.' + cookie.domain;
        break;

      case 'path':
        cookie.path = value || '';
        break;

      case 'secure':
        cookie.secure = true;
        break;

      case 'httponly':
        cookie.httpOnly = true;
        break;
    }
  }

  return cookie;
}

function serializePostData(params, headers) {
  (0, _utils.assert)((params.postData ? 1 : 0) + (params.jsonData ? 1 : 0) + (params.formData ? 1 : 0) + (params.multipartData ? 1 : 0) <= 1, `Only one of 'data', 'form' or 'multipart' can be specified`);

  if (params.jsonData) {
    var _contentType, _headers$_contentType;

    const json = JSON.stringify(params.jsonData);
    (_headers$_contentType = headers[_contentType = 'content-type']) !== null && _headers$_contentType !== void 0 ? _headers$_contentType : headers[_contentType] = 'application/json';
    return Buffer.from(json, 'utf8');
  } else if (params.formData) {
    var _contentType2, _headers$_contentType2;

    const searchParams = new URLSearchParams();

    for (const {
      name,
      value
    } of params.formData) searchParams.append(name, value);

    (_headers$_contentType2 = headers[_contentType2 = 'content-type']) !== null && _headers$_contentType2 !== void 0 ? _headers$_contentType2 : headers[_contentType2] = 'application/x-www-form-urlencoded';
    return Buffer.from(searchParams.toString(), 'utf8');
  } else if (params.multipartData) {
    var _contentType3, _headers$_contentType3;

    const formData = new _formData.MultipartFormData();

    for (const field of params.multipartData) {
      if (field.file) formData.addFileField(field.name, field.file);else if (field.value) formData.addField(field.name, field.value);
    }

    (_headers$_contentType3 = headers[_contentType3 = 'content-type']) !== null && _headers$_contentType3 !== void 0 ? _headers$_contentType3 : headers[_contentType3] = formData.contentTypeHeader();
    return formData.finish();
  } else if (params.postData) {
    var _contentType4, _headers$_contentType4;

    (_headers$_contentType4 = headers[_contentType4 = 'content-type']) !== null && _headers$_contentType4 !== void 0 ? _headers$_contentType4 : headers[_contentType4] = 'application/octet-stream';
    return Buffer.from(params.postData, 'base64');
  }

  return undefined;
}