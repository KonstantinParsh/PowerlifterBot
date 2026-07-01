using System.ComponentModel.DataAnnotations;
using PowerlifterBot.Enums;

namespace PowerlifterBot.Record;

public class UserProfile
{
    [Key]
    public long TelegramId { get; set; }
    
    public string Name { get; set; }
    public int Age { get; set; }
    public double BodyWeight { get; set; }
    public int Height { get; set; }
    public WeightUnit WeightUnit { get; set; }
}