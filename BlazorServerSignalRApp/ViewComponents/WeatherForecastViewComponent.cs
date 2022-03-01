using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerSignalRApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace BlazorServerSignalRApp.ViewComponents
{
    public class WeatherForecastViewComponent : ViewComponent
    {
        private WeatherForecastService _weatherForecast;

        public WeatherForecastViewComponent(WeatherForecastService weatherForecast)
        {
            _weatherForecast = weatherForecast;
        }
        public async Task<IViewComponentResult> InvokeAsync(DateTime start)
        {
            var forecasts = await _weatherForecast.GetForecastAsync(start);
            return View(forecasts);// Here we can specify view too
        }
    }
}