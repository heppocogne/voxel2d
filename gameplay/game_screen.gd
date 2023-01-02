extends Control

onready var viewport:Viewport=$ViewportContainer/Viewport


func _ready():
	get_tree().root.connect("size_changed", self, "_on_Window_size_changed")


func _on_Window_size_changed():
	viewport.size=OS.window_size
