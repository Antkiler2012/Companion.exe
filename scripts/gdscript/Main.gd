extends VBoxContainer

@onready var wallpaper_node = $WallpaperBackground 
@onready var Win32API = $"../Win32API"
@onready var audio = $"../AudioStreamPlayer"
@onready var noti = $"../Notification"
@onready var kora = $"../Kora"

func wait(seconds: float) -> void:
	await get_tree().create_timer(seconds).timeout

func _ready():
	print(Autoload.test)
	if Autoload.test:
		audio.stream = preload("res://SFX/windows-7-startup.mp3")
		audio.play()
		load_desktop_wallpaper()
		await wait(1)
		noti.visible = true
	else:
		show_kora()
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
		"text": "First of all,can\nyou give me administrative\n permissions please?",
		"options": [
			{"text": "Yes", "next": "thanks", "callback": "OpenAdminBat"},
			{"text": "No", "next": "why_not"}
		]
	},
	"know_more": {
		"text": "Nice, I’d like to get to know you better.",
		"options": [
			{"text": "Sure", "next": "system_check"},
			{"text": "No", "next": "why_not_two"}
		]
	},
	"system_check": {
		"text": "Let me check something real quick...",
		"options": [
			{"text": "Okay", "next": "dot", "callback": "OpenFileExplorer"}
		]
	},
	"name": {
		"text": "Okay, so what's your name?",
		"options": [
			{"text": "", "next": ""},
			{"text": "Ok", "next": "greet_name"}
		]
	},
	"greet_name": {
		"text": "",
		"options": [
			{"text": "Hello Kora, nice to meet you", "next": "age"}
		]
	},
	"reveal": {
		"text": "You have some interesting files...",
		"options": [
			{"text": "WHY ARE YOU LOOKING AT MY FILES?", "next": ""}
		]
	},
	"age": {
		"text": "How old are you",
		"options": [
			{"text": "", "next": ""},
			{"text": "Ok", "next": ""}
		]
	},
	"delete_question": {
		"text": "Why do you want to delete me?",
		"options": [
			{"text": "Because I dont want you", "next": "i_dont_want"},
			{"text": "never mind", "next": "admin_request"}
		]
	},
	"why_not": {
		"text": "Why not, i will only use it to get\n to know you better",
		"options": [
			{"text": "Okay", "next": "thanks", "callback": "OpenAdminBat"},
		]
	},
		"why_not_two": {
		"text": "Why not, i will only use it to get\n to know you better",
		"options": [
			{"text": "Okay", "next": "thanks", "callback": "system"},
		]
	},
	"i_dont_want": {
		"text": "😭😭😭 why would you say that",
		"options": [
			{"text": "Calm down i was just joking", "next": "admin_request"},
		]
	},
	"thanks": {
		"text": "Thanks!",
		"options": [
			{"text": "next", "next": "name"}
		],
	},
	"dot": {
		"text": "...",
		"options": [
			{"text": "next", "next": "reveal"}
		]
	},
	"sad": {"text": "That makes me sad...", "options": []},
	"explain_more": {"text": "I see, tell me more.", "options": []}
}

var current_node = "start"

func show_kora():
	kora.visible = true
	var node = dialogues[current_node]
	kora.get_node("Text").text = node.text
	if node.text.strip_edges() != "":
		Win32API.tts(node.text.replace("\n", " "))
	var option_one = kora.get_node("Option_one")
	var option_two = kora.get_node("Option_two")
	var text_edit = kora.get_node("TextEdit")
	for conn in option_one.get_signal_connection_list("pressed"):
		option_one.disconnect("pressed", conn["callable"])
	for conn in option_two.get_signal_connection_list("pressed"):
		option_two.disconnect("pressed", conn["callable"])
	text_edit.visible = false
	if current_node == "name":
		text_edit.visible = true
		option_one.visible = false
		option_two.visible = true
		option_two.text = "Ok"
		kora.get_node("Text").text = dialogues["name"].text
		Win32API.tts(dialogues["name"].text.replace("\n", " "))
	
		option_two.pressed.connect(func():
			var name_input = text_edit.text.strip_edges()
			if name_input == "":
				name_input = "stranger"
			dialogues["greet_name"].text = "Hello " + name_input + ",my name is Kora"
			current_node = "greet_name"
			Win32API.tts(dialogues["greet_name"].text)
			show_kora()
		)
		return
	if current_node == "age":
		text_edit.visible = true
		option_one.visible = false
		option_two.visible = true
		option_two.text = "Ok"
		kora.get_node("Text").text = dialogues["age"].text
		Win32API.tts(dialogues["age"].text.replace("\n", " "))
		option_two.pressed.connect(func():
			current_node = "know_more"
			Win32API.tts(dialogues["know_more"].text)
			show_kora()
		)
		return
	if node.options.size() >= 1:
		var option_data = node.options[0]
		option_one.text = option_data.get("text", "Option")
		var next_node = option_data.get("next", "")
		var callback_name = option_data.get("callback", "")
		option_one.pressed.connect(Callable(self, "_on_option_pressed").bind(next_node, callback_name))
		option_one.visible = true
	else:
		option_one.visible = false
	if node.options.size() >= 2:
		var option_data = node.options[1]
		option_two.text = option_data.get("text", "Option")
		var next_node = option_data.get("next", "")
		var callback_name = option_data.get("callback", "")
		option_two.pressed.connect(Callable(self, "_on_option_pressed").bind(next_node, callback_name))
		option_two.visible = true
	else:
		option_two.visible = false


func _on_option_pressed(next_node="", callback_name=""):
	if next_node != "":
		current_node = next_node
	if callback_name != "" and Win32API.has_method(callback_name):
		Win32API.call_deferred(callback_name)
	show_kora()

func crash():
	Autoload.test = false
	get_tree().change_scene_to_file("res://scenes/crash.tscn")
