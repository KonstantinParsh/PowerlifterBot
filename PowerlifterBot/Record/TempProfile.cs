using PowerlifterBot.Enums;

namespace PowerlifterBot.Record;

public class TempProfile
{
    public string Name { get; set; }
    public WeightUnit WeightUnit { get; set; }
    public int WelcomeMessageId { get; set; }
    
    private int _age;
    private double _bodyWeight;
    private int _height;

    public int Age
    {
        get { return _age; }
        set
        {
            if (value < 10) throw new ArgumentException("Вы должны быть старше 10!");
            if (value > 99) throw new ArgumentException("По моему, В слишком старый.");
            _age = value;
        }
    }

    public double BodyWeight
    {
        get { return _bodyWeight; }
        set
        {
            if (value < 40) throw new ArgumentException("Вес должен быть больше 40!");
            _bodyWeight = value;
        }
    }

    public int Height
    {
        get { return _height; }
        set
        {
            if (value < 150) throw new ArgumentException("Вы должны быть выше 150 см!");
            _height = value;
        }
    }
}