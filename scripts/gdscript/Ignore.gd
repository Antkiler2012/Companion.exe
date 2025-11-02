extends Button
@onready var info = $"../../ignore_info"
@onready var audio = $"../../AudioStreamPlayer"
func _ready():
	connect("pressed", Callable(self, "_on_pressed"))

func _on_pressed():
	info.visible = true
	audio.stream = preload("res://SFX/error.mp3")
	audio.play()
