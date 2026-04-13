using System.Drawing;

namespace ex_plorer
{
    internal struct IconPair
    {
        internal Icon Small;
        internal Icon Large;

        internal IconPair(Icon small, Icon large)
        {
            Small = small;
            Large = large;
        }
    }
}
