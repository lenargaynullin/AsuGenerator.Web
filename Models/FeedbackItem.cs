namespace AsuGenerator.Web.Models
{
    public class FeedbackItem
    {
        public string Name { get; set; } = "";        // Имя инженера / Название компании
        public string Role { get; set; } = "Инженер";  // Должность (Проектировщик, ГИП, Щитовик)
        public string City { get; set; } = "Казань";  // Город
        public string Message { get; set; } = "";     // Текст отзыва или предложения
        public DateTime Date { get; set; } = DateTime.Now; // Дата отправки
        public string Rating { get; set; } = "5";     // Оценка (число строк звезд)
    }
}