namespace Ensconce.NDjango.Custom
{
    public class ErrorTemplate
    {
        public bool Invoked;

        public override string ToString()
        {
            Invoked = true;
            return "{RandomText-668B7536-C32B-4D86-B065-70C143EB4AD9}";
        }

        public override bool Equals(object obj)
        {
            Invoked = true;
            return false;
        }

        public override int GetHashCode()
        {
            Invoked = true;
            return -1;
        }
    }
}
