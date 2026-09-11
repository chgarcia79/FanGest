using System;
using LibreHardwareMonitor.Hardware;

class TestProbe
{
    static void Main()
    {
        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsMotherboardEnabled = true,
            IsGpuEnabled = true,
            IsControllerEnabled = false,
            IsMemoryEnabled = false,
            IsStorageEnabled = false,
            IsNetworkEnabled = false
        };

        try
        {
            computer.Open();
            Console.WriteLine("Computer opened successfully!");
            foreach (IHardware hw in computer.Hardware)
            {
                hw.Update();
                Console.WriteLine("Hardware: " + hw.Name + " (" + hw.HardwareType + ") - " + hw.Identifier);
                foreach (ISensor sensor in hw.Sensors)
                {
                    if (sensor.SensorType == SensorType.Fan || sensor.SensorType == SensorType.Control)
                    {
                        Console.WriteLine(string.Format("   [{0}] {1}: {2} (Value: {3})", sensor.SensorType, sensor.Name, sensor.Identifier, sensor.Value));
                    }
                }
                foreach (IHardware sub in hw.SubHardware)
                {
                    sub.Update();
                    Console.WriteLine("  SubHardware: " + sub.Name + " (" + sub.HardwareType + ") - " + sub.Identifier);
                    foreach (ISensor sensor in sub.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Fan || sensor.SensorType == SensorType.Control)
                        {
                            Console.WriteLine(string.Format("     [{0}] {1}: {2} (Value: {3})", sensor.SensorType, sensor.Name, sensor.Identifier, sensor.Value));
                        }
                    }
                }
            }
            computer.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
