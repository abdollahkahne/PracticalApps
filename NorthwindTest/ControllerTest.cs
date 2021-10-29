using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PracticalApp.NorthwindMVC.Controllers;

namespace NorthwindTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestMethod]
        public void CanExecuteIndex()
        {
            var controller = new HomeController(null, null, null);
            var result = controller.Privacy();

            //Tests
            Assert.IsNotNull(result); // Test that result is not null
            Assert.IsInstanceOfType(result, typeof(ViewResult)); // Test that it returns a view
        }

    }
}