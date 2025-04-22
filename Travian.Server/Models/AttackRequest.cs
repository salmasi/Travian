public class AttackRequest
{
    public Guid AttackerVillageId { get; set; }
    public Guid DefenderVillageId { get; set; }
    public string AttackType { get; set; }
    public List<AttackTroopRequest> Troops { get; set; }
}

public class AttackTroopRequest
{
    public string TroopType { get; set; }
    public int Quantity { get; set; }
}

public class AttackResult
{
    public bool Success { get; set; }
    public Guid AttackId { get; set; }
    public DateTime ArrivalTime { get; set; }
    public double Distance { get; set; }
}