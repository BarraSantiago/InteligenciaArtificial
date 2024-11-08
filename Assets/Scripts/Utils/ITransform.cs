using System;

namespace Utils
{
    public class ITransform<TVector> 
        where TVector : IVector, IEquatable<TVector>
    {
        public TVector position;
        public TVector forward;
        public IVector up;
    }
}