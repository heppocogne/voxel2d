class_name CraftItemView
extends CenterContainer

onready var popup:PopupPanel=$PopupPanel
var _str_size:Vector2


func set_item_info(item_name:String,item_texture:Texture,n:int):
	$TextureRect/Label.text=str(n)
	$TextureRect.texture=item_texture
	$PopupPanel/Label.text=item_name
	_str_size=$PopupPanel/Label.get_font("font").get_string_size(item_name)


func _process(_delta:float):
	if Rect2(Vector2.ZERO,rect_size).has_point(get_local_mouse_position()):
		popup.popup(Rect2(get_global_mouse_position()+Vector2(16,0),_str_size))
	else:
		popup.hide()
