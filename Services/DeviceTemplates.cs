using AsuGenerator.Web.Services;

public static class DeviceTemplates
{
    public static DeviceDefinition GetQS1()
    {
        return new DeviceDefinition
        {
            Designation = "QS1",
            Name = "Выключатель-разъединитель",
            Article = "17014DEK",
            Manufacturer = "Dekraft",
            Type = "Switch",
            Pins = new List<PinDefinition>
            {
                new() { Number = "1", Label = "L1 вход", Direction = "In" },
                new() { Number = "2", Label = "L2 вход", Direction = "In" },
                new() { Number = "3", Label = "L3 вход", Direction = "In" },
                new() { Number = "4", Label = "N вход", Direction = "In" },
                new() { Number = "5", Label = "L1 выход", Direction = "Out" },
                new() { Number = "6", Label = "L2 выход", Direction = "Out" },
                new() { Number = "7", Label = "L3 выход", Direction = "Out" },
                new() { Number = "8", Label = "N выход", Direction = "Out" },
            }
        };
    }

    public static DeviceDefinition GetQF1()
    {
        return new DeviceDefinition
        {
            Designation = "QF1",
            Name = "Автомат защиты двигателя",
            Article = "21231DEK",
            Manufacturer = "Dekraft",
            Type = "Breaker",
            Pins = new List<PinDefinition>
            {
                new() { Number = "1", Label = "L1 вход", Direction = "In" },
                new() { Number = "2", Label = "L2 вход", Direction = "In" },
                new() { Number = "3", Label = "L3 вход", Direction = "In" },
                new() { Number = "4", Label = "L1 выход", Direction = "Out" },
                new() { Number = "5", Label = "L2 выход", Direction = "Out" },
                new() { Number = "6", Label = "L3 выход", Direction = "Out" },
            }
        };
    }
}