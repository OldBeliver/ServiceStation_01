using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStation_01
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceCenter serviceCenter = new ServiceCenter();
            serviceCenter.Work();
        }
    }

    class ServiceCenter
    {
        private int _money;
        private Queue<Car> _cars;
        private Store _store;
        private int _carsNumber;
        private int _storeMaxCapacity;
        private List<PerformedWork> _performedWorks;
        private int _penalty;

        public ServiceCenter()
        {
            _money = 0;
            _carsNumber = 5;
            _storeMaxCapacity = _carsNumber * 4;
            _cars = new Queue<Car>();
            _store = new Store(_storeMaxCapacity);
            _performedWorks = new List<PerformedWork>();
            _penalty = 200;
        }

        public void Work()
        {
            CreateCar(_carsNumber);

            Console.WriteLine($"станция технического обслуживания \"на все СТО\"");
            Console.WriteLine($"\nна сегодня записано {_cars.Count} машин");

            _store.MakeOrder();


            while (_cars.Count > 0)
            {
                Console.Clear();
                ShowTitle();

                _store.ShowInfo();
                Console.WriteLine();

                Car car = _cars.Dequeue();
                car.ShowInfo();

                CarRepair(car);
            }

            Console.Write($"рабочий день окончен\nнажмите любую для завершения ...");
            Console.ReadKey();
        }

        private void CreateCar(int number)
        {
            for (int i = 0; i < number; i++)
            {
                _cars.Enqueue(new Car());
            }
        }

        private void CarRepair(Car car)
        {
            bool doRepair = true;
            int detailIndex;
            detailIndex = 0;
            int currentPay = 0;
            _performedWorks.Clear();

            while (doRepair)
            {
                Console.Clear();
                ShowTitle();
                Console.WriteLine();
                _store.ShowInfo();
                Console.WriteLine();
                car.ShowInfo();

                Console.Write($"\n1-8 - номер детали со склада для замены\nnext - закончить ремонт\nВведите команду: ");
                string userInput = Console.ReadLine();

                if (userInput == "next")
                {
                    _money -= CalculatePenalty(car);
                    _money += CalculateCosts();
                    doRepair = false;
                }
                else
                {
                    bool result = int.TryParse(userInput, out int number);

                    if (result)
                    {
                        int upperLimit = _store.GetCount();

                        if (number > 0 && number <= upperLimit)
                        {
                            detailIndex = number - 1;
                        }
                        else
                        {
                            result = false;
                        }
                    }

                    if (result && (!_store.AvailableQuantity(detailIndex) || !car.AvaliableCondition(detailIndex)))
                    {
                        Console.WriteLine($"штраф");
                        currentPay = _store.GetPrice(detailIndex) * (-1);
                        DateTime timeNow = DateTime.Now;
                        _performedWorks.Add(new PerformedWork((currentPay), _store.GetName(detailIndex), timeNow));
                    }

                    if (result && _store.AvailableQuantity(detailIndex) && car.AvaliableCondition(detailIndex))
                    {
                        int newCondition = _store.GetNewDetailCondition(detailIndex);
                        car.ReplaceDetail(detailIndex, newCondition);
                        currentPay = _store.GetPrice(detailIndex) * 3 / 2;
                        DateTime timeNow = DateTime.Now;
                        _performedWorks.Add(new PerformedWork((currentPay), _store.GetName(detailIndex), timeNow));
                    }
                    currentPay = 0;
                    Console.WriteLine($"выполненные работы");
                    for (int i = 0; i < _performedWorks.Count; i++)
                    {
                        _performedWorks[i].ShowInfo();
                    }
                }

                Console.Write($"любую для продолжения ... ");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private void ShowTitle()
        {
            Console.WriteLine($"Автосервис\tбаланс {_money} рублей");
            Console.WriteLine($"машин в очереди {_cars.Count}");
        }

        private int CalculateCosts()
        {
            return _performedWorks.Sum(work => work.Price);
        }

        private int CalculatePenalty(Car car)
        {
            return car.failRepair() * _penalty;
        }
    }

    abstract class AbstractCreator
    {
        protected List<Detail> _details;
        public AbstractCreator()
        {
            _details = new List<Detail>();
        }

        protected abstract BrakePads CreateBrakePads();
        protected abstract DiskBrake CreateDiskBrake();
        protected abstract ElectricGenerator CreateElectricGenerator();
        protected abstract FuelPump CreateFuelPump();
        protected abstract Plug CreatePlug();
        protected abstract ScheduledMintenance CreateScheduledMintenance();
        protected abstract Snubber CreateSnubber();
        protected abstract TimingBelt CreateTimingBelt();
    }

    class PerformedWork
    {
        public int Price { get; private set; }
        public string Detail { get; private set; }
        public DateTime Time { get; private set; }

        public PerformedWork(int price, string detail, DateTime time)
        {
            Price = price;
            Detail = detail;
            Time = time;
        }

        public void ShowInfo()
        {
            Console.WriteLine($"замена {Detail}, {Price} рублей, время {Time}");
        }
    }

    class Store : AbstractCreator
    {
        private int _capacity;
        private int _maxCapacity;

        public Store(int maxCapacity) : base()
        {
            _details.Add(CreateBrakePads());
            _details.Add(CreateDiskBrake());
            _details.Add(CreateElectricGenerator());
            _details.Add(CreateFuelPump());
            _details.Add(CreatePlug());
            _details.Add(CreateScheduledMintenance());
            _details.Add(CreateSnubber());
            _details.Add(CreateTimingBelt());

            _capacity = 0;
            _maxCapacity = maxCapacity;
        }

        public void ShowInfo()
        {
            _capacity = CalculateCapacity();

            Console.WriteLine($"Вместимость склада {_capacity}/{_maxCapacity}");
            Console.WriteLine($"№   название\t\tкол-во\tцена\tработа");
            Console.WriteLine($"-----------------------------------------------");
            for (int i = 0; i < _details.Count; i++)
            {
                Console.Write($"{i + 1:d2}. ");
                _details[i].ShowInfo();
            }
        }

        public void MakeOrder()
        {
            bool isFillUp = true;

            string userInput = "";

            while (isFillUp)
            {
                Console.WriteLine();
                ShowInfo();

                if (_capacity == _maxCapacity)
                {
                    Console.WriteLine($"\nна складе нет места");
                }

                Console.WriteLine($"\nexit - завершить наполнение склада\nenter - продолжить наполнение склада");
                userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "exit":
                        isFillUp = false;
                        break;
                    case "test":
                        for (int i = 0; i < _details.Count; i++)
                        {
                            _details[i].SetTestQuantity();
                        }
                        break;
                    default:
                        ChangeQuantity();
                        break;
                }
            }
        }

        public int GetPrice(int index)
        {
            return _details[index].Price;
        }

        public string GetName(int index)
        {
            return _details[index].Name;
        }

        public int GetCount()
        {
            return _details.Count;
        }

        public int GetNewDetailCondition(int index)
        {
            return _details[index].Condition;
        }

        public bool AvailableQuantity(int index)
        {
            return _details[index].Quantity > 0;
        }

        private void ChangeQuantity()
        {
            int freePlaces = _maxCapacity - _capacity;
            Console.Write($"Введите номер позиции товара на складе: ");

            if (GetNumber(out int number))
            {
                if (number >= 1 && number <= _details.Count)
                {
                    int index = number - 1;
                    Console.WriteLine($"{_details[index].Name}: {_details[index].Quantity}");

                    Console.Write($"Добавить количество: ");
                    if (GetNumber(out int value))
                    {
                        if (value <= freePlaces)
                        {
                            _details[index].SetQuantity(value);
                            _capacity += value;
                        }
                        else
                        {
                            Console.WriteLine($"превышен лимит склада");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ошибка ввода количества");
                    }
                }
                else
                {
                    Console.WriteLine($"число вне диапазона");
                }
            }
            else
            {
                Console.WriteLine($"ошибка ввода");
            }
            Console.Write($"любую для продолжения ...");
            Console.ReadKey();
            Console.Clear();
        }

        private int CalculateCapacity()
        {
            int currentCapacity = _details.Sum(detail => detail.Quantity);

            return currentCapacity;
        }

        public bool GetNumber(out int number)
        {
            bool result = int.TryParse(Console.ReadLine(), out number);

            return result;
        }

        protected override BrakePads CreateBrakePads()
        {
            return new NewBrakePads();
        }

        protected override DiskBrake CreateDiskBrake()
        {
            return new NewDiskBrake();
        }

        protected override ElectricGenerator CreateElectricGenerator()
        {
            return new NewElectricGenerator();
        }

        protected override FuelPump CreateFuelPump()
        {
            return new NewFuelPump();
        }

        protected override Plug CreatePlug()
        {
            return new NewPlug();
        }

        protected override ScheduledMintenance CreateScheduledMintenance()
        {
            return new NewScheduledMintenance();
        }

        protected override Snubber CreateSnubber()
        {
            return new NewSnubber();
        }

        protected override TimingBelt CreateTimingBelt()
        {
            return new NewTimibgBelt();
        }
    }

    class Car : AbstractCreator
    {
        private int _minCondition;
        public Car() : base()
        {
            _minCondition = 20;

            _details.Add(CreateBrakePads());
            _details.Add(CreateDiskBrake());
            _details.Add(CreateElectricGenerator());
            _details.Add(CreateFuelPump());
            _details.Add(CreatePlug());
            _details.Add(CreateScheduledMintenance());
            _details.Add(CreateSnubber());
            _details.Add(CreateTimingBelt());
        }

        public void ShowInfo()
        {
            var filteredByCondition = _details.OrderBy(detail => detail.Condition).ToList();
            Console.WriteLine($"название\t\tсостояние");
            for (int i = 0; i < filteredByCondition.Count; i++)
            {
                filteredByCondition[i].ShowInfo(_minCondition);
            }
        }

        public void GetBrokenDetail()
        {
            var brokenDetail = from Detail detail in _details
                               where detail.Condition <= _minCondition
                               select detail;

            foreach (var detail in brokenDetail)
            {
                Console.WriteLine($"{detail.Name} - {detail.Condition}");
            }
        }

        public void ReplaceDetail(int index, int newCondition)
        {
            _details[index].SetNewCondition(newCondition);
        }

        public bool AvaliableCondition(int index)
        {
            return _details[index].Condition <= _minCondition;
        }
        public int GetPrice(int index)
        {
            return _details[index].Price;
        }

        public int failRepair()
        {
            var failRepair = _details.Where(detail => detail.Condition <= _minCondition);
            return failRepair.Count();
        }

        protected override BrakePads CreateBrakePads()
        {
            return new UsedBrakePads();
        }

        protected override DiskBrake CreateDiskBrake()
        {
            return new UsedDiskBrake();
        }

        protected override ElectricGenerator CreateElectricGenerator()
        {
            return new UsedElectricGenerator();
        }

        protected override FuelPump CreateFuelPump()
        {
            return new UsedFuelPump();
        }

        protected override Plug CreatePlug()
        {
            return new UsedPlug();
        }

        protected override ScheduledMintenance CreateScheduledMintenance()
        {
            return new UsedScheduledMintenance();
        }

        protected override Snubber CreateSnubber()
        {
            return new UsedSnuber();
        }

        protected override TimingBelt CreateTimingBelt()
        {
            return new UsedTimingBelt();
        }
    }

    abstract class Detail
    {
        protected static Random Rand = new Random();
        protected int Durable;

        public string Name { get; protected set; }
        public int Condition { get; protected set; }
        public int Price { get; protected set; }
        public int Quantity { get; protected set; }

        public Detail()
        {
            Condition = 100;
            Quantity = 0;
            Durable = 50;
        }

        public void ShowInfo()
        {
            Console.WriteLine($"{Name} \t   {Quantity} \t{Price}\t{Price / 2}");
        }

        public void ShowInfo(int minCondition)
        {
            ConsoleColor color;
            color = Console.ForegroundColor;

            if (Condition <= minCondition)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine($"{Name} \t{Condition:d2}");
            Console.ForegroundColor = color;
        }

        public void SetQuantity(int value)
        {
            Quantity += value;
        }

        public void SetTestQuantity()
        {
            Quantity = 5;
        }

        public void SetNewCondition(int value)
        {
            Condition = value;
        }
    }

    abstract class BrakePads : Detail
    {
        public BrakePads()
        {
            Name = "тормозные накладки";
        }
    }

    class NewBrakePads : BrakePads
    {
        public NewBrakePads()
        {
            Price = 328;
        }
    }

    class UsedBrakePads : BrakePads
    {
        public UsedBrakePads()
        {
            Condition = Rand.Next(Durable);
            Quantity = 1;
        }
    }

    abstract class DiskBrake : Detail
    {
        public DiskBrake()
        {
            Name = "тормозной диск ";
        }
    }

    class NewDiskBrake : DiskBrake
    {
        public NewDiskBrake()
        {
            Price = 765;
        }
    }

    class UsedDiskBrake : DiskBrake
    {
        public UsedDiskBrake()
        {
            Condition = Rand.Next(Durable);
            Quantity = 1;
        }
    }

    abstract class ElectricGenerator : Detail
    {
        public ElectricGenerator()
        {
            Name = "электро генератор";
        }
    }

    class NewElectricGenerator : ElectricGenerator
    {
        public NewElectricGenerator()
        {
            Price = 2424;
        }
    }

    class UsedElectricGenerator : ElectricGenerator
    {
        public UsedElectricGenerator()
        {
            Condition = Rand.Next(Durable);
            Quantity = 1;
        }
    }

    abstract class FuelPump : Detail
    {
        public FuelPump()
        {
            Name = "топливный насос";
        }
    }

    class NewFuelPump : FuelPump
    {
        public NewFuelPump()
        {
            Price = 770;
        }
    }

    class UsedFuelPump : FuelPump
    {
        public UsedFuelPump()
        {
            Condition = Rand.Next(Durable);
            Quantity = 1;
        }
    }

    abstract class Plug : Detail
    {
        public Plug()
        {
            Name = "свеча зажигания";
        }
    }

    class NewPlug : Plug
    {
        public NewPlug()
        {
            Price = 123;
        }
    }

    class UsedPlug : Plug
    {
        public UsedPlug()
        {
            Condition = Rand.Next(Durable);
            Quantity = 1;
        }
    }

    abstract class ScheduledMintenance : Detail
    {
        public ScheduledMintenance()
        {
            Name = "плановое ТО      ";
        }
    }

    class NewScheduledMintenance : ScheduledMintenance
    {
        public NewScheduledMintenance()
        {
            Price = 5000;
        }
    }

    class UsedScheduledMintenance : ScheduledMintenance
    {
        public UsedScheduledMintenance()
        {
            Condition = Rand.Next(Durable);
            Quantity = 1;
        }
    }

    abstract class Snubber : Detail
    {
        public Snubber()
        {
            Name = "амортизатор      ";
        }
    }

    class NewSnubber : Snubber
    {
        public NewSnubber()
        {
            Price = 685;
        }
    }

    class UsedSnuber : Snubber
    {
        public UsedSnuber()
        {
            Condition = Rand.Next(Durable);
            Quantity = 1;
        }
    }

    abstract class TimingBelt : Detail
    {
        public TimingBelt()
        {
            Name = "ремень ГРМ       ";
        }
    }

    class NewTimibgBelt : TimingBelt
    {
        public NewTimibgBelt()
        {
            Price = 255;
        }
    }

    class UsedTimingBelt : TimingBelt
    {
        public UsedTimingBelt()
        {
            Condition = Rand.Next(Durable);
            Quantity = 1;
        }
    }
}
