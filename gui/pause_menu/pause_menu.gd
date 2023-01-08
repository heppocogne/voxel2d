extends VBoxContainer

signal save_requested()


func _ready():
	pass


func _on_Continue_pressed():
	get_tree().paused=false
	queue_free()


func _on_BackToTitle_pressed():
	pass # Replace with function body.


func _on_SaveAndQuit_pressed():
	pass # Replace with function body.
