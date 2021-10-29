using System;

namespace Prime.Services
{
    public class PrimeService
    {
        public bool IsPrime(int candidate)
        {
            if (candidate < 2)
            {
                return false;
            }
            for (int devisor = 2; devisor <= Math.Sqrt(candidate); devisor++)
            {
                if (candidate % devisor == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
