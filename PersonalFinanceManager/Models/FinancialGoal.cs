using System.ComponentModel.DataAnnotations;
namespace PersonalFinanceManager.Models;
public class FinancialGoal { public int Id {get;set;} [Required,StringLength(80)] public string Name {get;set;}=string.Empty; [Range(1,9999999999)] public decimal TargetAmount {get;set;} [Range(0,9999999999)] public decimal CurrentAmount {get;set;} [DataType(DataType.Date)] public DateTime? TargetDate {get;set;} public string? UserId {get;set;} public DateTime CreatedAt {get;set;}=DateTime.UtcNow; public int Percentage=>TargetAmount==0?0:Math.Min(100,(int)Math.Round(CurrentAmount/TargetAmount*100)); }
