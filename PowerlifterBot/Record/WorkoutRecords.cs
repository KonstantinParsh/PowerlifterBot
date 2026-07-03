using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerlifterBot.Record;

public class WorkoutRecords
{
    private double _weight;
    private int _reps;
    private DateTime _date;
    
    [Key]
    public int Id { get; set; }
    
    public string ExerciseName { get; set; }

    public double Weight
    {
        get => _weight;
        set
        {
            if (value <= 0) throw new ArgumentException("Вес снаряда должен быть больше 0.");
            _weight = value;
        }
    }

    public int Reps
    {
        get => _reps;
        set
        {
            if (value <= 0) throw new ArgumentException("Количество повторений должно быть больше 0.");
            _reps = value;
        }
    }

    public DateTime Date
    {
        get => _date;
        set
        {
            if (value > DateTime.Now) throw new ArgumentException("Нельзя зафиксировать рекорд в будущем времени!");
            _date = value;
        }
    }
    
    
    public long TelegramId { get; set; }
    
    [ForeignKey(nameof(TelegramId))]
    public virtual UserProfile UserProfile { get; set; }
}