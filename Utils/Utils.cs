using System.Net.Mail;

namespace OwListy
{
    public static class Utils
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string GenerateRandomCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
