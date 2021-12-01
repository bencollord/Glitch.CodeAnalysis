using System;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IDefaultValueBuilder<TDerived> 
        where TDerived : IDefaultValueBuilder<TDerived>
    {
        TDerived HasDefaultValue(bool value);
        TDerived HasDefaultValue(int value);
        TDerived HasDefaultValue(short value);
        TDerived HasDefaultValue(long value);
        TDerived HasDefaultValue(uint value);
        TDerived HasDefaultValue(ushort value);
        TDerived HasDefaultValue(ulong value);
        TDerived HasDefaultValue(float value);
        TDerived HasDefaultValue(double value);
        TDerived HasDefaultValue(decimal value);
        TDerived HasDefaultValue(char value);
        TDerived HasDefaultValue(string value);
        TDerived HasDefaultValue(Enum value);
    }
}
