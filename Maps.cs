namespace Role_Playing_Game;

public static class Maps
{
	public static string GetMapTileRender(char[][] map, int tileI, int tileJ)
	{
		if (tileJ < 0 || tileJ >= map.Length || tileI < 0 || tileI >= map[tileJ].Length)
		{
			if (map == Field) return Sprites.Mountain;
			return Sprites.Open;
		}
				return map[tileJ][tileI] switch
		{
			'w' => Sprites.Water,
			'W' => Sprites.Wall_0000,
			'b' => Sprites.Building,
			't' => Sprites.Tree,
			' ' or 'X' => Sprites.Open,
			'i' => Sprites.Inn,
			's' => Sprites.Store,
			'f' => Sprites.Fence,
			'c' => Sprites.Chest,
			'e' => Sprites.EmptyChest,
			'B' => Sprites.CSB,
			'N' => Sprites.CSN,
			'A' => Sprites.CSA,
			'1' => tileJ < map.Length / 2 ? Sprites.ArrowUp : Sprites.ArrowDown,
			'm' => Sprites.Mountain,
			'0' => Sprites.Town,
			'g' => Sprites.Guard,
			'2' => Sprites.Castle,
			'p' => Sprites.Mountain2,
			'T' => Sprites.Tree2,
			'k' => Sprites.King,
			'h' => Sprites.Wall_0000,
			_ => Sprites.Error,
		};
		/*
		- Phương thức này nhận vào một bản đồ (mảng hai chiều kiểu char[][]), 
		và hai chỉ số tọa độ (I, J) của ô vuông trong bản đồ.
        - Mục đích của phương thức là trả về chuỗi ký tự biểu diễn hình ảnh (sprite) của ô vuông đó 
		dựa trên ký tự tại vị trí map[tileJ][tileI].
        - Nếu tọa độ nằm ngoài giới hạn của bản đồ, phương thức trả về sprite của núi hoặc ô trống tùy theo bản đồ hiện tại
		(nếu là Field thì trả về núi, còn lại trả về trống).
        - Cấu trúc switch kiểm tra giá trị của ký tự tại vị trí tileI, tileJ 
		trong bản đồ để trả về các hình ảnh tương ứng (ví dụ: 'w' đại diện cho nước, 'W' là tường, 'b' là tòa nhà,...).
		*/
	}

	public static bool IsValidCharacterMapTile(char[][] map, int tileI, int tileJ)
	{
		if (tileJ < 0 || tileJ >= map.Length || tileI < 0 || tileI >= map[tileJ].Length)
		{
			return false;
		}
		return map[tileJ][tileI] switch
		{
			'A' => true,
			'N' => true,
			'B' => true,
			' ' => true,
			'i' => true,
			's' => true,
			'c' => true,
			'e' => true,
			'1' => true,
			'0' => true,
			'g' => true,
			'2' => true,
			'X' => true,
			'k' => true,
			'h' => true,
			_ => false,
		};
		/*
		- Phương thức này cũng nhận vào một bản đồ và hai chỉ số tọa độ, 
		nhưng mục đích là kiểm tra xem ô vuông tại vị trí này có phải là ô mà nhân vật có thể đứng được hay không.
        - Nếu tọa độ vượt quá giới hạn bản đồ, nó trả về false, tức là ô này không hợp lệ.
        - Nếu ký tự trong ô thuộc danh sách các ký tự cho phép (ví dụ: 'A', 'N', 'B', 'i', 's', 'c',...),
		thì trả về true, nghĩa là ô này hợp lệ để nhân vật di chuyển tới.
		*/
	}

	public static readonly char[][] Town =
	[
		"   WWWWWWWWWWWWWWWWW111WWWWWWWWWWWWWWWWW   ".ToCharArray(),
		"wwwWbbbfbbb               bffbffbffb  cWwww".ToCharArray(),
		"wwwW                                   Wwww".ToCharArray(),
		"wwwW  T bfb T      bfNfb               Wwww".ToCharArray(),
		"wwwWcb                         T bBb T Wwww".ToCharArray(),
		"wwwW      TfAfT      X                 Wwww".ToCharArray(),
		"wwwW                                   Wwww".ToCharArray(),
		"wwwWbfbfb T     i    c    s     T bfbfbWwww".ToCharArray(),
		"wwwW                                  cWwww".ToCharArray(),
		"wwwWbffbfbffbfbfbbfb   bbfbfbffbfbbfbfbWwww".ToCharArray(),
		"   WWWWWWWWWWWWWWWWW111WWWWWWWWWWWWWWWWW   ".ToCharArray(),
		//readonly: Biến chỉ có thể được gán giá trị một lần,
		//          Sau khi gán, biến không thể thay đổi giá trị trong suốt thời gian tồn tại của chương trình.
	];

	public static readonly char[][] Field =
	[
		"mmmpmmmmpmmmmmpmmmmmpmmmmmpmmmpmmmpmmmpmm".ToCharArray(),
		"mmpppppppmmmpppmmmpppppmmppmmmpmmmmpppmmm".ToCharArray(),
		"mmpmmpmmpmppmmpmpmmpmmpmmmmmmpppmmpmpmmmp".ToCharArray(),
		"TTTTTc     mpmm    cTT           m2mcmmpp".ToCharArray(),
		"TTTT        mm                    g   mmm".ToCharArray(),
		"TTT   TT                 mm           mpm".ToCharArray(),
		"TTT           TTT      mmmm     TT    ppm".ToCharArray(),
		"www      T              mm     TTT    www".ToCharArray(),
		"www          TT    ww           T     www".ToCharArray(),
		"www                 ww  TTT         wwwww".ToCharArray(),
		"www   w0w      Tww                 mmmmmm".ToCharArray(),
		"wwww          wwwwwww      TT   cmmmmmmmm".ToCharArray(),
		"wwwwwwwwwwwwwwwwwwwwwTTTTTTTTTTTTmmmmmmmm".ToCharArray(),
		"wwwwwwwwwwwwwwwwwwwwTTTTTTTTTTTTTTmmmmmmm".ToCharArray(),
		"wwwwwwwwwwwwwwwwwwwTTTTTTTTTTTTTTTTmmmmmm".ToCharArray(),
	];

	public static readonly char[][] Castle =
	[
		"WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW".ToCharArray(),
		"WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW".ToCharArray(),
		"WWc                WkW                cWW".ToCharArray(),
		"WW                 W W                 WW".ToCharArray(),
		"WW                                     WW".ToCharArray(),
		"WW       W      h       W      W       WW".ToCharArray(),
		"WW                                     WW".ToCharArray(),
		"WW                                     WW".ToCharArray(),
		"WW       W      W       W      W       WW".ToCharArray(),
		"WW                                     WW".ToCharArray(),
		"WW                                     WW".ToCharArray(),
		"WW       W      W       W      W       WW".ToCharArray(),
		"WW                                     WW".ToCharArray(),
		"WWc                                   cWW".ToCharArray(),
		"WWWWWWWWWWWWWWWWWWW   WWWWWWWWWWWWWWWWWWW".ToCharArray(),
		"WWWWWWWWWWWWWWWWWWW111WWWWWWWWWWWWWWWWWWW".ToCharArray(),
	];
}
