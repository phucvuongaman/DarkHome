namespace DarkHome
{
    // Hợp đồng cho ai muốn lưu một cái gì đó vào json save game
    // Phải thêm dữ liệu riêng biệt vào SaveData
    public interface IDataPersistence
    {
        void SaveData(ref SaveData data);
        void LoadData(SaveData data);
    }
}