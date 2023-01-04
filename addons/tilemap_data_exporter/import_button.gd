tool
extends Button

var tilemap:TileMap
var tilemap_data:TileMapData


func _ready():
	pass


func _on_ImportButton_pressed(_tilemap:TileMap):
	tilemap=_tilemap
	$FileDialog.popup_centered(Vector2(600,400))


func import_tilemap_data(data:TileMapData):
	for id in data._map:
		for v in data._map[id]:
			tilemap.set_cellv(v,id)


func _on_FileDialog_file_selected(path:String):
	tilemap_data=load(path)
	if tilemap.get_used_rect().size!=Vector2.ZERO:
		$AcceptDialog.popup_centered(Vector2(300,200))
	else:
		import_tilemap_data(tilemap_data)


func _on_AcceptDialog_confirmed():
	for v in tilemap.get_used_rect():
		tilemap.set_cellv(v,-1)
	import_tilemap_data(tilemap_data)
