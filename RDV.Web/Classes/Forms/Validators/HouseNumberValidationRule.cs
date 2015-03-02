// ============================================================
// 
// 	RDV
// 	RDV.Web 
// 	HouseNumberValidationRule.cs
// 
// 	Created by: ykorshev 
// 	 at 28.06.2013 14:59
// 
// ============================================================
namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// Валидатор, использующийся для валидации правильного написания номера дома
    /// </summary>
    public class HouseNumberValidationRule: ValidationRule
    {
        /// <summary>
        /// Инициализирует правило валидации с указанными параметрами
        /// </summary>
        public HouseNumberValidationRule() : base("houseNumber", "true")
        {
        }
    }
}