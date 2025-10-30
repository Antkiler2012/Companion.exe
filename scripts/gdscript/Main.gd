extends VBoxContainer

@onready var wallpaper_node = $WallpaperBackground 
@onready var Win32API = $"../Win32API"
func _ready():
	load_desktop_wallpaper()

func load_desktop_wallpaper():
	var path = Win32API.GetDesktopWallpaperPath()
	if path and FileAccess.file_exists(path):
		var image = Image.new()
		var error = image.load(path)
		if error == OK:
			var texture = ImageTexture.create_from_image(image)
			wallpaper_node.texture = texture    
		else:
			wallpaper_node.modulate = Color(0, 0, 0, 1)     
	else:
		wallpaper_node.modulate = Color(0, 0, 0, 1)
