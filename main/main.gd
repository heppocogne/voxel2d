extends Control

var delta_sum:=0.0
onready var logo:TextureRect=$CenterContainer/VBoxContainer/TextureRect


func _ready():
	randomize()
	_on_Main_resized()


func _on_Main_resized():
	$TextureRect.rect_position=(rect_size-$TextureRect.rect_size)/2


func _on_NewGame_pressed():
	get_tree().change_scene_to(preload("res://gameplay/game_screen.tscn"))


func _on_LoadGame_pressed():
	pass # Replace with function body.


func _on_Exit_pressed():
	get_tree().quit(0)
