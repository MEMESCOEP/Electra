namespace PiShockDesktop
{
    public class NumHelpers
    {
        public static float DistanceBetweenNumbers(float a, float b)
        {
            if (a > b)
            {
                // If 'a' is greater than 'b', return the difference of 'a' and 'b' multiplied by 2
                return (a - b) * 2f;
            }

            // If 'a' is not greater than 'b', return the difference of 'b' and 'a'
            return b - a;
        }
    }
}
