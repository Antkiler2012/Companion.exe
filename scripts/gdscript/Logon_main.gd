extends VBoxContainer

@onready var wallpaper_node = $WallpaperBackground 
@onready var Win32API = $"../Win32API"
@onready var icon_node = $"../User_Icon/TextureRect"

func wait(seconds: float) -> void:
	await get_tree().create_timer(seconds).timeout

func _ready():
	load_desktop_wallpaper()
	load_user_image()

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
		
func load_user_image():
	var img = Win32API.GetWindowsUserIcon()
	var texture = ImageTexture.create_from_image(img)
	icon_node.texture = texture

func typehello():
	Win32API.OpenNotepadAndTypeHello()
