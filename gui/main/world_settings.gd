extends PopupDialog

onready var warning:Label=$MarginContainer/VBoxContainer/HBoxContainer2/Warning
onready var generate_button:Button=$MarginContainer/VBoxContainer/HBoxContainer4/Generate


func _ready():
	pass


func _on_WorldName_text_changed(new_text:String):
	var dir:=Directory.new()
	if new_text=="" or dir.file_exists("user://worlds/"+new_text+"/world"):
		warning.visible=true
		generate_button.disabled=true
	else:
		warning.visible=false
		generate_button.disabled=false


func _on_Generate_pressed():
	var game_screen:Control
	if $MarginContainer/VBoxContainer/HBoxContainer3/Normal.pressed:
		game_screen=preload("res://gameplay/game_screen.tscn").instance()
	get_tree().root.add_child(game_screen)
	var world:=game_screen.get_node("ViewportContainer/Viewport/World")
	world.WorldName=$MarginContainer/VBoxContainer/HBoxContainer/WorldName.text
	world.NewWorld()
	game_screen.get_node("ViewportContainer/GUILayer/GUI").setup()
	get_tree().root.get_node("Main").queue_free()


func _on_Cancel_pressed():
	hide()
