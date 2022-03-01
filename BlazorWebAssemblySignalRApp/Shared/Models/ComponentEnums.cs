using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebAssemblySignalRApp.Shared.Models
{
    // Here we defined enum inside class like a class inside a class. It is not a member of class just it is there and to get it we should use the class staticly to have them or use full name of clas
    public class ComponentEnums
    {
        public enum Manufacturer { SpaceX, NASA, ULA, VirginGalactic, Unknown }
        public enum Engine { Ion, Plasma, Fusion, Warp }
        public enum Color { ImperialRed, SpacecruiserGreen, StarshipBlue, VoyagerOrange }
        public enum Degree { Diploma, Bachelor, Master, Phd }
        public enum Major { Mathematics, Physics, ChimicalEng, MechanicalEng, StructuralEng, SystemEng, ElecticalEng }
        public enum Sports { Football, Wrestling, Swimming, Jumping, Boxing }
        public class InsideClass
        {
            public Engine Engine { get; set; }
            private Engine getEngine()
            {
                return Engine;
            }
        }

    }
    public class InsideClass
    {
        public ComponentEnums.Color Color { get; set; }
    }
    public class ColorfullEngine
    {
        InsideClass? Inside { get; set; }
        ComponentEnums.InsideClass? _Inside { get; set; }
        ComponentEnums.Color Color { get; set; }
        ComponentEnums.Engine Engine { get; set; }
        ComponentEnums.Manufacturer Manufacturer { get; set; }
        private void LogMembers()
        {
            var x = new ComponentEnums();
            x.ToString(); // x has not any member except inherited from obj
        }
    }
}