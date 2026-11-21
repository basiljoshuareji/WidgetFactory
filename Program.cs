
// Widget
class Widget
{
    public int Id;
    public int ArrivalTime;

    public int AStart, AEnd;
    public int BStart, BEnd;
    public int CStart, CEnd;
}

class FinishProcessingEvent
{
    public Machine machine;
    public Widget widget;
    public int FinishTime;

    public FinishProcessingEvent(Machine m, Widget w, int time)
    {
        machine = m;
        widget = w;
        FinishTime = time;
    }
}

//Machine
// every machine should have a queue, a boolean isBusy, and a variable to hold the maximum queue it ever witnessed

class Machine
{
    public string Name;
    public int ProcessingTime;

    public Queue<Widget> Queue = new();

    public bool isBusy = false;
    public int maxQueue = 0;

    public Machine(string name, int time)
    {
        Name = name;
        ProcessingTime = time;
    }
// add teh widget to the queue, if queue length is larger than maxQueue update it, call function to process next queue
    public void AddWidget(Widget w, Factory f)
    {
        Queue.Enqueue(w);
        maxQueue = Math.Max(maxQueue, Queue.Count);
        ProcessNext(f);

    }
// if machine is busy return
// if not, dequeue a widget and check if the widget is waiting for which machine.
// record its start timing
// flag the machine as busy
    public void ProcessNext(Factory f)
    {
        if (isBusy || Queue.Count ==0)
        {
            return;
        }

        Widget w = Queue.Dequeue();
        isBusy = true;

        if (Name == "A")
        {
            w.AStart = f.Time;
        }
        else if (Name == "B")
        {
            w.BStart = f.Time;
        }
        else if (Name == "C")
        {
            w.CStart = f.Time;
        }

        f.events.Enqueue(new FinishProcessingEvent(this, w, f.Time + ProcessingTime),f.Time + ProcessingTime);

    }

// after finishing each widget - record its end time, add the widget to the next machine
// if the widget comes out of machine C, add it to the completed queue
    public void Finish(Widget w, Factory f)
    {
        if (Name == "A")
        {
            w.AEnd = f.Time;
            f.B.AddWidget(w, f);
        }

        if (Name == "B")
        {
            w.BEnd = f.Time;
            f.C.AddWidget(w, f);
        }

        if (Name == "C")
        {
            w.CEnd = f.Time;
            f.completed.Add(w);
        }

        isBusy = false;

        ProcessNext(f);
    }
}
// creates 3 machines as objects
// crates lists completed and arrived which would be used in the report to calculate the number of widgets arrived and completed
// run the program from 0 to 480 , at every 30 milliseconds, create a widget and enqueue it to A's queue
class Factory
{
    public int Time = 0;

    public Machine A = new ("A" , 15);
    public Machine B = new ("B" , 30);
    public Machine C = new ("C" , 45);

    public List<Widget> completed = new();
    public List<Widget> arrived = new();
    public PriorityQueue<FinishProcessingEvent, int> events = new();

    public async Task Run()
    {
        int id=1;
        
        for(int t=0; t<480 ; t++)
        {
            Time =t;

            if (t%30 == 0)
            {
                Widget w = new Widget
                {
                    Id = id++,
                    ArrivalTime = t
                };

                arrived.Add(w);
                A.AddWidget(w, this);
            }

            while (events.Count > 0 &&
                   events.Peek().FinishTime <= Time)
            {
                var e = events.Dequeue();
                e.machine.Finish(e.widget, this);
            }

            await Task.Delay(10);
        }
    }
// Create the report using a string.
// append the dialogues to the string
// write the total arrived and total completed
// write average using function 
    public void Report()
    {
        string report = "";

        report += "FACTORY SIMULATION REPORT\n\n";

        report += $"Widgets Arrived: {arrived.Count}\n";
        report += $"Widgets Completed: {completed.Count}\n\n";

        report += "MACHINE STATUS\n";

        report += $"Machine A: Queue = {A.Queue.Count}\n";
        report += $"Machine B: Queue = {B.Queue.Count}\n";
        report += $"Machine C: Queue = {C.Queue.Count}\n\n";

        report += 
             $"Average Time in Factory: {completed.Average(w => w.CEnd - w.ArrivalTime)} minutes\n\n";

        report += "MAXIMUM QUEUE LENGTH\n";

        report += $"Machine A: {A.maxQueue}\n";
        report += $"Machine B: {B.maxQueue}\n";
        report += $"Machine C: {C.maxQueue}\n";

        File.WriteAllText("FactoryReport.txt", report);

        Console.WriteLine(report);
        Console.WriteLine("Report saved!");
    }
}

class Program
{
    static async Task Main()
    {
        Factory factory = new Factory();

        await factory.Run();

        factory.Report();
    }
}
