extends VBoxContainer

@onready var wallpaper_node = $WallpaperBackground 
@onready var Win32API = $"../Win32API"
@onready var audio = $"../AudioStreamPlayer"
@onready var noti = $"../Notification"
@onready var kora = $"../Kora"
func wait(seconds: float) -> void:
	await get_tree().create_timer(seconds).timeout

func _ready():
	audio.stream = preload("res://SFX/windows-7-startup.mp3")
	audio.play()
	load_desktop_wallpaper()
	await wait(10)
	noti.visible = true

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
		
func typehello():
	Win32API.OpenNotepadAndTypeHello()

var dialogues = {
	"start": {
		"text": "Hello, I will be\nyour mental\nhealth assistant",
		"options": [
			{"text": "Hello", "next": "admin_request"},
			{"text": "How do I delete you?", "next": "delete_question"}
		]
	},
	"admin_request": {
		"text": "First of all, can you give me administrative permissions please?",
		"options": [
			{"text": "Yes", "next": "thanks"},
			{"text": "No", "next": "why_not"}
		]
	},
	"delete_question": {
		"text": "Why do you want to delete me?",
		"options": [
			{"text": "Because I can", "next": "sad"},
			{"text": "Just curious", "next": "explain_more"}
		]
	},
	"thanks": {"text": "Thanks!", "options": []},
	"why_not": {"text": "Oh, okay...", "options": []},
	"sad": {"text": "That makes me sad...", "options": []},
	"explain_more": {"text": "I see, tell me more.", "options": []}
}

var current_node = "start"

func show_kora():
	kora.visible = true
	var node = dialogues[current_node]
	kora.get_node("Text").text = node.text
	Win32API.tts(node.text.replace("\n", " "))

	var option_one = kora.get_node("Option_one")
	var option_two = kora.get_node("Option_two")

	for conn in option_one.get_signal_connection_list("pressed"):
		option_one.disconnect("pressed", conn["callable"])
	for conn in option_two.get_signal_connection_list("pressed"):
		option_two.disconnect("pressed", conn["callable"])

	if node.options.size() >= 1:
		option_one.text = node.options[0].text
		option_one.pressed.connect(Callable(self, "_on_option_pressed").bind(node.options[0].next))
		option_one.visible = true
	else:
		option_one.visible = false

	if node.options.size() >= 2:
		option_two.text = node.options[1].text
		option_two.pressed.connect(Callable(self, "_on_option_pressed").bind(node.options[1].next))
		option_two.visible = true
	else:
		option_two.visible = false

func _on_option_pressed(next_node):
	current_node = next_node
	show_kora()
