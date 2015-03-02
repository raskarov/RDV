// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	SystemStatsType.cs
// 
// 	Created by: ykorshev 
// 	 at 25.10.2013 11:24
// 
// ============================================================
namespace RDV.Domain.Enums
{
    /// <summary>
    /// 
    /// </summary>
    public enum SystemStatsType: short
    {
        [EnumDescription("Количество запросов")]
        RequestsCount = 1,

        [EnumDescription("Количество объектов Мультилистинга")]
        MultilistingsObjectCount = 2,

        [EnumDescription("Количество cовместных сделок")]
        IntersystemDealsCount = 3,

        [EnumDescription("Количество авторизовавшихся пользователей")]
        AuthorizedUsersCount = 4,

        [EnumDescription("Количество активированных пользователей")]
        ActivateUsersCount = 5,

        [EnumDescription("Число зарегистрированных пользователей за день")]
        RegistredUsersCount = 6,

        [EnumDescription("Среднее время продажи объекта, дней")]
        ObjectSellDaysCount = 7,

        [EnumDescription("Количество платных услуг")]
        ServicesCount = 8

    }
}