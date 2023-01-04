tool
class_name AnimatedTexture2
extends AnimatedTexture

export var base_texture:Texture
export(int,1,256) var h_frames:int=1 setget set_h_frames
export(int,1,256) var v_frames:int=1 setget set_v_frames
enum FrameDirections{
	HORIZONTAL,
	VERTICAL,
}
export(FrameDirections) var frame_direction:int=HORIZONTAL setget set_frame_direction
export var back_and_forth:bool=false

var atlas:Array


func _init():
	frames=h_frames*v_frames
	atlas=[]


func set_h_frames(h:int):
	h_frames=h
	frames=h_frames*v_frames
	atlas=[]


func set_v_frames(v:int):
	v_frames=v
	frames=h_frames*v_frames
	atlas=[]


func set_frame_direction(d:int):
	frame_direction=d
	atlas=[]


func get_frame_texture(frame:int)->Texture:
	if atlas.size()==0:
		var tex_size:=base_texture.get_size()/Vector2(h_frames,v_frames)
		if frame_direction==HORIZONTAL:
			for y in range(v_frames):
				for x in range(h_frames):
					atlas.push_back(_texture_at(x,y,tex_size))
		else:
			for x in range(h_frames):
				for y in range(v_frames):
					atlas.push_back(_texture_at(x,y,tex_size))

	return atlas[frame]


func _texture_at(x:int,y:int,size:Vector2)->AtlasTexture:
	var atlas:=AtlasTexture.new()
	atlas.atlas=base_texture
	atlas.region=Rect2(Vector2(x,y),size)
	return atlas
