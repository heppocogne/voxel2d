extends Control


func _ready():
	randomize()
	get_tree().change_scene_to(preload("res://gameplay/game_screen.tscn"))
