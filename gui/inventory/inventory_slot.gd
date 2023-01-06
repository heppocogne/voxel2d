class_name InventorySlot
extends TextureRect

var display_name:String

onready var item_texture:TextureRect=$CenterContainer/TextureRect
onready var label:Label=$Label
onready var popup:PopupPanel=$PopupPanel


func _ready():
	$PopupPanel/Label.text=display_name


func clear():
	set_item_info("",null,0)


func set_item_info(item_name:String,texture:Texture,n:int):
	display_name=item_name
	$PopupPanel/Label.text=display_name
	item_texture.texture=texture
	set_item_quantity(n)


func _process(_delta:float):
	if Rect2(rect_position,rect_size).has_point(get_local_mouse_position()):
		popup.popup(Rect2(get_local_mouse_position(),$PopupPanel/Label.get_font("font").get_string_size(display_name)))
	else:
		popup.hide()


func set_item_quantity(n:int):
	if n==0 or n==1:
		label.text=""
	else:
		label.text=str(n)
