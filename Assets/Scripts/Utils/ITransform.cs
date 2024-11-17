using System;

namespace Utils
{
    public class ITransform<TVector>
        where TVector : IVector, IEquatable<TVector>
    {
        public TVector position { get; set; }
        public TVector forward;
    }
}