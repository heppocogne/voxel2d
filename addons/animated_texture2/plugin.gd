tool
extends EditorPlugin


func _enter_tree():
	add_custom_type("AnimatedTexture2","AnimatedTexture",preload("res://addons/animated_texture2/animated_texture2.gd"),null)
	pass


func _exit_tree():
	remove_custom_type("AnimatedTexture2")
	pass
