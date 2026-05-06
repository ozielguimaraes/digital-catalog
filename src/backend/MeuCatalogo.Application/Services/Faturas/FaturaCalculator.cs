namespace MeuCatalogo.Application.Services.Faturas;

public static class FaturaCalculator
{
    public static (DateTime DataInicio, DateTime DataFim, DateTime DataVencimento) Calcular(
        byte diaFechamento,
        byte diaVencimento,
        int mes,
        int ano)
    {
        if (diaFechamento is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(diaFechamento));
        if (diaVencimento is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(diaVencimento));
        if (mes is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(mes));

        var primeiroDoMes = new DateTime(ano, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var mesAnterior = primeiroDoMes.AddMonths(-1);

        var fechamentoMesAnterior = new DateTime(
            mesAnterior.Year,
            mesAnterior.Month,
            Math.Min(diaFechamento, DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month)),
            0, 0, 0, DateTimeKind.Utc);

        var dataInicio = fechamentoMesAnterior.AddDays(1);

        var dataFim = new DateTime(
            primeiroDoMes.Year,
            primeiroDoMes.Month,
            Math.Min(diaFechamento, DateTime.DaysInMonth(primeiroDoMes.Year, primeiroDoMes.Month)),
            0, 0, 0, DateTimeKind.Utc);

        var dataVencimento = new DateTime(
            primeiroDoMes.Year,
            primeiroDoMes.Month,
            Math.Min(diaVencimento, DateTime.DaysInMonth(primeiroDoMes.Year, primeiroDoMes.Month)),
            0, 0, 0, DateTimeKind.Utc);

        if (dataVencimento < dataFim)
            dataVencimento = dataVencimento.AddMonths(1);

        return (dataInicio, dataFim, dataVencimento);
    }

    public static (int Mes, int Ano) ProximoMes(int mes, int ano)
        => mes == 12 ? (1, ano + 1) : (mes + 1, ano);

    public static (int Mes, int Ano) FaturaParaData(byte diaFechamento, DateTime data)
    {
        if (diaFechamento is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(diaFechamento));

        var diaCorte = Math.Min(diaFechamento, DateTime.DaysInMonth(data.Year, data.Month));
        if (data.Day <= diaCorte)
            return (data.Month, data.Year);

        return ProximoMes(data.Month, data.Year);
    }
}
