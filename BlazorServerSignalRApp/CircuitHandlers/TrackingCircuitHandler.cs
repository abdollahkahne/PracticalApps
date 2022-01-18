using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorServerSignalRApp.CircuitHandlers
{
    public class TrackingCircuitHandler : CircuitHandler
    {
        private HashSet<Circuit> _circuits = new();

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return base.OnCircuitClosedAsync(circuit, cancellationToken);
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _circuits.Remove(circuit);
            Console.WriteLine(circuit.Id);
            Console.WriteLine($"Number of Connected Cirecuits After Disconnect: {_circuits.Count}");
            return base.OnConnectionDownAsync(circuit, cancellationToken);
        }

        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _circuits.Add(circuit);
            Console.WriteLine(circuit.Id);
            Console.WriteLine($"Number of Connected Cirecuits After Connect: {_circuits.Count}");
            return base.OnConnectionUpAsync(circuit, cancellationToken);
        }
    }
}