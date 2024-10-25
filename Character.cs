namespace Role_Playing_Game;

public class Character
{
	public string? Name {get; set;} = "";  //Dấu '?' ở đây cho phép biến nhận giá trị null
	public string? PlaceOfBirth {get; set;} = "";
	public string? Time { get; set; } = "";
	public int Year {get; set; } = 0;
	public string? Major {get; set; } = "";
	public long StudentID {get; set;} = 0;
	public string[] GraduationRanking = {
		"Trung Bình",
		"Khá", 
		"Giỏi",
		"Xuất Sắc"
	};
	public int Level { get; set; } = 1;
	public int ScoreB { get; set; } = 0;
	public int ScoreN { get; set; } = 0;
	public int ScoreA { get; set; } = 0;
	public int HighScore { get; set; } = 0;
	public int Experience { get; set; }
	public int ExperienceToNextLevel { get; set; } = 10;
	public int Health { get; set; } = 5;

	//Được viết theo cú pháp expression-bodied member (cú pháp viết gọn cho các phương thức hoặc thuộc tính đơn giản).
	//Cú pháp '=>'  trả về giá trị mỗi khi property hoặc phương thức được gọi, mà không lưu giá trị.
	public int MaxHealth => Level * 5;
	public int Gold { get; set; }
	public int Damage { get; set; } = 1; 

	//I, J: Vị trí của nhân vật theo tọa độ pixel trong bản đồ hiện tại.
	public int I { get; set; }
	public int J { get; set; }
	
	/*
	TileI, TileJ: Vị trí của nhân vật theo tọa độ ô (tile) trên bản đồ. Công thức chia nhỏ tọa độ pixel để lấy tọa độ ô.
    Nếu I hoặc J nhỏ hơn 0, tọa độ ô được tính theo cách điều chỉnh để phù hợp với giá trị âm.
	*/
	public int TileI => I < 0 ? (I - 6) / 7 : I / 7;
	public int TileJ => J < 0 ? (J - 3) / 4 : J / 4;
	private string[]? _mapAnaimation;
	public string[]? MapAnimation
	{
		get => _mapAnaimation;
		set
		{
			_mapAnaimation = value;
			_mapAnimationFrame = 0;
		}
	}
	//"_mapAnimationFram" là một biến riêng tư kiểu số nguyên, 
	//(_) ở đầu tên biến được dùng để phân biệt giữa các biến với các tham số hoặc thuộc tính khác trong cùng lớp.
	//Dùng để theo dõi trạng thái hoặc khung hình hiện tại trong một hoạt ảnh của bản đồ trong game.
	private int _mapAnimationFrame;

	/*
	- MapAnimation: Mảng chứa các chuỗi mô tả hoạt ảnh nhân vật, chẳng hạn như đang chạy hoặc đứng yên.
    Khi giá trị mới được gán vào MapAnimation, MapAnimationFrame sẽ được reset về 0 để bắt đầu lại từ đầu.
	- _mapAnimationFrame: Chỉ số khung hình hiện tại của hoạt ảnh.
    - MapAnimationFrame: Khi chỉ số khung hình thay đổi, 
	kiểm tra nếu đã kết thúc hoạt ảnh hiện tại thì chuyển nhân vật sang trạng thái đứng yên (Idle).
    Ví dụ: nếu hoạt ảnh hiện tại là "RunUp" (chạy lên) thì khi kết thúc, 
	nhân vật sẽ chuyển sang "IdleUp" (đứng yên quay mặt lên trên).
	*/
	public int MapAnimationFrame
	{
		get => _mapAnimationFrame;
		set
		{
			_mapAnimationFrame = value;
			Moved = false;
			if (_mapAnimationFrame >= MapAnimation!.Length)
			{
				if (MapAnimation == Sprites.RunUp) { Moved = true; MapAnimation = Sprites.IdleUp; }
				if (MapAnimation == Sprites.RunDown) { Moved = true; MapAnimation = Sprites.IdleDown; }
				if (MapAnimation == Sprites.RunLeft) { Moved = true; MapAnimation = Sprites.IdleLeft; }
				if (MapAnimation == Sprites.RunRight) { Moved = true; MapAnimation = Sprites.IdleRight; }
				_mapAnimationFrame = 0;
			}
		}
	}

	//IsIdle: Kiểm tra xem nhân vật có đang ở trạng thái đứng yên (Idle) hay không
	//dựa trên hoạt ảnh hiện tại (IdleUp, IdleDown, IdleLeft, IdleRight).
	public bool IsIdle
	{
		get =>
			_mapAnaimation == Sprites.IdleDown ||
			_mapAnaimation == Sprites.IdleUp ||
			_mapAnaimation == Sprites.IdleLeft ||
			_mapAnaimation == Sprites.IdleRight;
	}

	//Render: Phương thức này trả về chuỗi mô tả hình dạng của nhân vật dựa trên khung hoạt ảnh hiện tại.
	//Nếu không có hoạt ảnh nào được xác định, nhân vật sẽ được vẽ ở tư thế mặc định ("T-pose").
	public string Render =>
		_mapAnaimation is not null && _mapAnimationFrame < _mapAnaimation.Length
		? _mapAnaimation[_mapAnimationFrame]
		: // "T" pose :D
		  @" __O__ " + '\n' +
		  @"   |   " + '\n' +
		  @"   |   " + '\n' +
		  @"  | |  ";
	public bool Moved { get; set; } = false;
}
