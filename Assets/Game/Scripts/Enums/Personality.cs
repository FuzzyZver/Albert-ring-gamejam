using UnityEngine;

public enum Gender { Male, Female }

public enum TraitId
{
    Proud,      // Гордый
    Greedy,     // Алчный
    Cunning,    // Хитрый
    Pious,      // Набожный
    Cruel,      // Жестокий
    Craven,     // Трусливый
    Lustful,    // Похотливый
    Drunkard,   // Пьяница
    Obsessed,   // Одержимый
    Honest      // Честный
}

// ВАЖНО: новые значения дописывать ТОЛЬКО в конец. Unity сериализует enum числом,
// вставка в середину молча перемешает всё, что уже настроено в ассетах.
public enum VerbId
{
    Flatter,          // Польстить
    Bribe,            // Подкупить
    Threaten,         // Пригрозить
    DrinkTogether,    // Выпить вместе
    Seduce,           // Соблазнить
    Insult,           // Послать
    FulfillAmbition,  // Исполнить желание
    AskForTroops,     // Просить войск

    PrayTogether,     // Помолиться вместе
    HuntTogether,     // Позвать на охоту
    InviteToCastle,   // Позвать в замок
    AskForCounsel     // Спросить совета
}

public enum AmbitionId
{
    MarryMyDaughter,    // женись на моей дочери
    GiveMeTheMill,      // отдай мельницу
    GrantMeATitle,      // дай титул
    KillMyRival,        // убей моего врага
    BuildTheChapel,     // построй часовню
    HearMyProphecy,     // выслушай пророчество
    TasteMySoup,        // попробуй мой суп
    NameYourDogAfterMe  // назови собаку в мою честь
}

public enum ConsequenceId
{
    None,
    ChallengeToDuel,   // вызов на поединок
    Scandal,           // скандал, узнают все
    SpreadRumor,       // слух: −мнение у остальных
    DemandGift,        // требует подарок
    LeaveCourt,        // уезжает, войск не будет
    PlotMurder,        // начинает готовить убийство
    GetDrunk,          // напивается, теряешь действие
    PublicRepentance,  // кается прилюдно, +крестьяне
    FallDownStairs,    // падает с лестницы
    BringExtraTroops,  // приведёт больше копий
    TellRival,         // доносит сопернику
    // смертельные
    ChokeAtFeast,        // подавился на собственном пиру
    FallFromHorse,       // лошадь решила иначе
    PoisonedByLover,     // любовник передумал
    StruckByLightning,   // пророчество Одо сбылось буквально
    PlagueInCastle,      // крысы пришли раньше армии
    DuelGoesBadly,       // ты принял вызов. Зря
    DrownInMoat,         // ночью, пьяным, во рву собственного замка
    AvengedBySon,        // сын оскорблённого лорда оказался терпелив

    // смешные и разрушительные
    LordDiesLaughing,    // лорд умер со смеху над твоим предложением
    BastardClaimsCastle, // объявился ещё один наследник
    LoversMeet,          // два любовника встретились в коридоре
    DogBitesNamesake,    // собака укусила лорда, в честь которого названа
    ExcommunicatedByBrother, // брат Одо отлучил тебя лично
    HorseEatsTheTreaty,  // договор был на столе. Лошадь была рядом
    TurnipToTheFace,     // крестьянин промахнулся. Или нет
    SoupWasTerrible,     // ты попробовал. Ты сказал правду
    ProphecyFulfilled,   // Одо был прав, и это хуже всего
    ConfessDrunkenly     // ты рассказал всё. Всем
}