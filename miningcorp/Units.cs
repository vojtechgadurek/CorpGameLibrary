namespace GameCorpLib
{
	//This code was GENERATED!!! Please make changes  at DoubleClassGenerator project in DoubleClassGenerator.cs


	public struct Oil
	{
		public double Value;
		public Oil(double value) { Value = value; }
		public override string ToString() { return Value.ToString(); }
		public static Oil operator -(Oil a) => new Oil(-(double)a);
		public static Oil operator +(Oil a, Oil b) => new Oil((double)a + (double)b);
		public static Oil operator +(Oil a, double b) => new Oil((double)a + (double)b);
		public static Oil operator +(double a, Oil b) => new Oil((double)a + (double)b);
		public static Oil operator +(Oil a, int b) => new Oil((double)a + (double)b);
		public static Oil operator +(int a, Oil b) => new Oil((double)a + (double)b);
		public static Oil operator -(Oil a, Oil b) => new Oil((double)a - (double)b);
		public static Oil operator -(Oil a, double b) => new Oil((double)a - (double)b);
		public static Oil operator -(double a, Oil b) => new Oil((double)a - (double)b);
		public static Oil operator -(Oil a, int b) => new Oil((double)a - (double)b);
		public static Oil operator -(int a, Oil b) => new Oil((double)a - (double)b);
		public static Oil operator /(Oil a, Oil b) => new Oil((double)a / (double)b);
		public static Oil operator /(Oil a, double b) => new Oil((double)a / (double)b);
		public static Oil operator /(double a, Oil b) => new Oil((double)a / (double)b);
		public static Oil operator /(Oil a, int b) => new Oil((double)a / (double)b);
		public static Oil operator /(int a, Oil b) => new Oil((double)a / (double)b);
		public static Oil operator *(Oil a, Oil b) => new Oil((double)a * (double)b);
		public static Oil operator *(Oil a, double b) => new Oil((double)a * (double)b);
		public static Oil operator *(double a, Oil b) => new Oil((double)a * (double)b);
		public static Oil operator *(Oil a, int b) => new Oil((double)a * (double)b);
		public static Oil operator *(int a, Oil b) => new Oil((double)a * (double)b);
		public static bool operator <(Oil a, Oil b) => ((double)a < (double)b);
		public static bool operator <(Oil a, double b) => ((double)a < (double)b);
		public static bool operator <(double a, Oil b) => ((double)a < (double)b);
		public static bool operator <(Oil a, int b) => ((double)a < (double)b);
		public static bool operator <(int a, Oil b) => ((double)a < (double)b);
		public static bool operator >(Oil a, Oil b) => ((double)a > (double)b);
		public static bool operator >(Oil a, double b) => ((double)a > (double)b);
		public static bool operator >(double a, Oil b) => ((double)a > (double)b);
		public static bool operator >(Oil a, int b) => ((double)a > (double)b);
		public static bool operator >(int a, Oil b) => ((double)a > (double)b);
		public static bool operator ==(Oil a, Oil b) => ((double)a == (double)b);
		public static bool operator ==(Oil a, double b) => ((double)a == (double)b);
		public static bool operator ==(double a, Oil b) => ((double)a == (double)b);
		public static bool operator ==(Oil a, int b) => ((double)a == (double)b);
		public static bool operator ==(int a, Oil b) => ((double)a == (double)b);
		public static bool operator !=(Oil a, Oil b) => ((double)a != (double)b);
		public static bool operator !=(Oil a, double b) => ((double)a != (double)b);
		public static bool operator !=(double a, Oil b) => ((double)a != (double)b);
		public static bool operator !=(Oil a, int b) => ((double)a != (double)b);
		public static bool operator !=(int a, Oil b) => ((double)a != (double)b);
		public static bool operator >=(Oil a, Oil b) => ((double)a >= (double)b);
		public static bool operator >=(Oil a, double b) => ((double)a >= (double)b);
		public static bool operator >=(double a, Oil b) => ((double)a >= (double)b);
		public static bool operator >=(Oil a, int b) => ((double)a >= (double)b);
		public static bool operator >=(int a, Oil b) => ((double)a >= (double)b);
		public static bool operator <=(Oil a, Oil b) => ((double)a <= (double)b);
		public static bool operator <=(Oil a, double b) => ((double)a <= (double)b);
		public static bool operator <=(double a, Oil b) => ((double)a <= (double)b);
		public static bool operator <=(Oil a, int b) => ((double)a <= (double)b);
		public static bool operator <=(int a, Oil b) => ((double)a <= (double)b);
		public static implicit operator Oil(double a) => new Oil(a);
		public static explicit operator double(Oil a) => (double)a.Value;
		public static implicit operator Oil(int a) => new Oil(a);

	};
	public struct Money
	{
		public double Value;
		public Money(double value) { Value = value; }
		public override string ToString() { return Value.ToString(); }
		public static Money operator -(Money a) => new Money(-(double)a);
		public static Money operator +(Money a, Money b) => new Money((double)a + (double)b);
		public static Money operator +(Money a, double b) => new Money((double)a + (double)b);
		public static Money operator +(double a, Money b) => new Money((double)a + (double)b);
		public static Money operator +(Money a, int b) => new Money((double)a + (double)b);
		public static Money operator +(int a, Money b) => new Money((double)a + (double)b);
		public static Money operator -(Money a, Money b) => new Money((double)a - (double)b);
		public static Money operator -(Money a, double b) => new Money((double)a - (double)b);
		public static Money operator -(double a, Money b) => new Money((double)a - (double)b);
		public static Money operator -(Money a, int b) => new Money((double)a - (double)b);
		public static Money operator -(int a, Money b) => new Money((double)a - (double)b);
		public static Money operator /(Money a, Money b) => new Money((double)a / (double)b);
		public static Money operator /(Money a, double b) => new Money((double)a / (double)b);
		public static Money operator /(double a, Money b) => new Money((double)a / (double)b);
		public static Money operator /(Money a, int b) => new Money((double)a / (double)b);
		public static Money operator /(int a, Money b) => new Money((double)a / (double)b);
		public static Money operator *(Money a, Money b) => new Money((double)a * (double)b);
		public static Money operator *(Money a, double b) => new Money((double)a * (double)b);
		public static Money operator *(double a, Money b) => new Money((double)a * (double)b);
		public static Money operator *(Money a, int b) => new Money((double)a * (double)b);
		public static Money operator *(int a, Money b) => new Money((double)a * (double)b);
		public static bool operator <(Money a, Money b) => ((double)a < (double)b);
		public static bool operator <(Money a, double b) => ((double)a < (double)b);
		public static bool operator <(double a, Money b) => ((double)a < (double)b);
		public static bool operator <(Money a, int b) => ((double)a < (double)b);
		public static bool operator <(int a, Money b) => ((double)a < (double)b);
		public static bool operator >(Money a, Money b) => ((double)a > (double)b);
		public static bool operator >(Money a, double b) => ((double)a > (double)b);
		public static bool operator >(double a, Money b) => ((double)a > (double)b);
		public static bool operator >(Money a, int b) => ((double)a > (double)b);
		public static bool operator >(int a, Money b) => ((double)a > (double)b);
		public static bool operator ==(Money a, Money b) => ((double)a == (double)b);
		public static bool operator ==(Money a, double b) => ((double)a == (double)b);
		public static bool operator ==(double a, Money b) => ((double)a == (double)b);
		public static bool operator ==(Money a, int b) => ((double)a == (double)b);
		public static bool operator ==(int a, Money b) => ((double)a == (double)b);
		public static bool operator !=(Money a, Money b) => ((double)a != (double)b);
		public static bool operator !=(Money a, double b) => ((double)a != (double)b);
		public static bool operator !=(double a, Money b) => ((double)a != (double)b);
		public static bool operator !=(Money a, int b) => ((double)a != (double)b);
		public static bool operator !=(int a, Money b) => ((double)a != (double)b);
		public static bool operator >=(Money a, Money b) => ((double)a >= (double)b);
		public static bool operator >=(Money a, double b) => ((double)a >= (double)b);
		public static bool operator >=(double a, Money b) => ((double)a >= (double)b);
		public static bool operator >=(Money a, int b) => ((double)a >= (double)b);
		public static bool operator >=(int a, Money b) => ((double)a >= (double)b);
		public static bool operator <=(Money a, Money b) => ((double)a <= (double)b);
		public static bool operator <=(Money a, double b) => ((double)a <= (double)b);
		public static bool operator <=(double a, Money b) => ((double)a <= (double)b);
		public static bool operator <=(Money a, int b) => ((double)a <= (double)b);
		public static bool operator <=(int a, Money b) => ((double)a <= (double)b);
		public static implicit operator Money(double a) => new Money(a);
		public static explicit operator double(Money a) => (double)a.Value;
		public static implicit operator Money(int a) => new Money(a);
		public static explicit operator int(Money a) => (int)a.Value;
	};

	public static class NumberExtension
	{
		public static Oil Oil(this double value) => new Oil(value);
		public static Oil Oil(this int value) => new Oil(value);
		public static Money Money(this double value) => new Money(value);
		public static Money Money(this int value) => new Money(value);
	}
}
