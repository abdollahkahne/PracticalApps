using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyPattern.Strategies;

namespace StrategyPattern
{
    public class DriverQueue
    {
        private static Queue<Type> _queue = new Queue<Type>();
        static DriverQueue()
        {
            _queue.Enqueue(typeof(StrategyA));
            _queue.Enqueue(typeof(StrategyB));
            _queue.Enqueue(typeof(StrategyC));
            _queue.Enqueue(typeof(StrategyC));
            _queue.Enqueue(typeof(StrategyA));
            _queue.Enqueue(typeof(StrategyC));
            _queue.Enqueue(typeof(StrategyA));
            _queue.Enqueue(typeof(StrategyC));
        }
        public static Type TryDequeue()
        {
            Type type;
            if (!_queue.TryDequeue(out type))
            {
                // throw new InvalidOperationException();
                return null;
            }
            return type;
        }
    }
}