extends Control

export var viewport_scale:=2.0
onready var viewport:Viewport=$ViewportContainer/Viewport


func _ready():
	get_tree().root.connect("size_changed", self, "_on_Window_size_changed")
	_on_Window_size_changed()
	OS.min_window_size=Vector2(352,100)


func _on_Window_size_changed():
	viewport.set_size_override(true,OS.window_size/viewport_scale)
	viewport.set_size_override_stretch(true)
	viewport.size=OS.window_size
