using System.Collections.Generic;

namespace Ensconce.NDjango.Tests.Data
{
    public class AthleteList : System.Collections.IEnumerable
    {
        public AthleteList()
        {
            list = new List<Athlete>(new Athlete[] { new Athlete("Michael", "Jordan"), new Athlete("Magic", "Johnson") });
        }

        private readonly List<Athlete> list;

        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion IEnumerable Members

        private struct Athlete
        {
            public Athlete(string fstName, string lstName)
            {
                FirstName = fstName; LastName = lstName;
            }

            public string FirstName;
            public string LastName;
            public string name { get { return FirstName + ' ' + LastName; } }
        }
    }
}
