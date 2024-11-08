using System;

namespace Utils
{
    public class ITransform<TVector> 
        where TVector : IVector, IEquatable<TVector>
    {
        public TVector position;
    }
}