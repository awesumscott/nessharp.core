using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NESSharp.Core.Models;

public class SuperTileConfigModel {
	public List<GenericTileModel> SuperTiles { get; set; }
	public List<ScreenModel> Screens { get; set; }
}
public class MegaTileConfigModel {
	//public static MetaTileConfigModel Load(string json) => Json.LoadString<MetaTileConfigModel>(json);
	public List<GenericTileModel> SuperTiles { get; set; }
	public List<GenericTileModel> MegaTiles { get; set; }
	public List<ScreenModel> Screens { get; set; }
}

/// <summary>
/// Generic model for Tiles and any other superset of meta-tile. Collision and Palette values are
/// optional, and can be used in any way by the implementing background module.
/// </summary>
public class GenericTileModel {
	public U8 Id { get; set; }
	public string Name { get; set; }
	public List<U8> Tiles { get; set; }
	public U8 Collision { get; set; }
	public U8 Palette { get; set; }
}

public class ScreenModel {
	public U8 Id { get; set; }
	public string Name { get; set; }
	public List<U8> Tiles { get; set; }
	[MinLength(4)]
	[MaxLength(4)]
	public U8[] Neighbors { get; set; }
}
