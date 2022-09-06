using System;
using System.IO;
using System.Linq;
using Unosquare.PiGpio;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.RaspberryIO.Peripherals;

namespace Pyro.IO.GPIO
{
    public class Controller : GpioController
    {
        public DhtSensor TSensor;
        public Controller()
        {
            //Gpio07
            Enum.TryParse<BcmPin>(File.ReadAllText("PinConfig.txt"), out var pin);
            TSensor = DhtSensor.Create(DhtType.Dht11, base[pin]);
            TSensor.Start();
        }
    }
}