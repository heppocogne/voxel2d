extends VBoxContainer

onready var world_node:=get_tree().root.get_node("GameScreen/ViewportContainer/Viewport/World")


func _ready():
	pass


func _on_Continue_pressed():
	get_tree().paused=false
	queue_free()


func _on_BackToTitle_pressed():
	get_tree().paused=false
	world_node.SaveWorld()
	get_tree().root.get_node("GameScreen").queue_free()
	get_tree().change_scene_to(load("res://main/main.tscn"))
	queue_free()


func _on_SaveAndQuit_pressed():
	world_node.SaveWorld()
	get_tree().quit(0)
