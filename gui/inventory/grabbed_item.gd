class_name GrabbedItem
extends Sprite

var display_name:String
var quantity:int=0
onready var label:Label=$Label
onready var popup_panel:PopupPanel=$PopupPanel

var _str_size:Vector2


func _ready():
	if display_name!="":
		popup_panel.popup(Rect2(
			get_global_mouse_position(),
			_str_size))


func clear():
	set_item_info("",null,0)


func _process(_delta:float):
	position=get_global_mouse_position()
	popup_panel.rect_position=position+Vector2(16,0)


func set_item_info(item_name:String,item_texture:Texture,n:int):
	display_name=item_name
	if display_name!="":
		$PopupPanel/Label.text=display_name
		_str_size=$PopupPanel/Label.get_font("font").get_string_size(display_name)
	texture=item_texture
	set_item_quantity(n)


func set_item_quantity(n:int):
	quantity=n
	if n==0 or n==1:
		$Label.text=""
	else:
		$Label.text=str(n)
