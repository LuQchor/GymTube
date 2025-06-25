export const mapVideoApiToFrontend = (apiVideo: any) => {
  return {
    id: apiVideo.Id || apiVideo.id,
    title: apiVideo.Title || apiVideo.title,
    description: apiVideo.Description || apiVideo.description,
    muxPlaybackId: apiVideo.MuxPlaybackId || apiVideo.muxPlaybackId,
    thumbnailUrl: apiVideo.ThumbnailUrl || apiVideo.thumbnailUrl,
    fileSize: apiVideo.FileSize || apiVideo.fileSize,
    duration: apiVideo.Duration || apiVideo.duration,
    createdAt: apiVideo.CreatedAt || apiVideo.createdAt,
    updatedAt: apiVideo.UpdatedAt || apiVideo.updatedAt,
    likes: apiVideo.Likes || apiVideo.likes,
    dislikes: apiVideo.Dislikes || apiVideo.dislikes,
    userVote: apiVideo.UserVote || apiVideo.userVote,
    isPremium: apiVideo.IsPremium || apiVideo.isPremium,
    isPrivate: apiVideo.IsPrivate || apiVideo.isPrivate,
    uploadStatus: apiVideo.UploadStatus || apiVideo.uploadStatus,
    user: apiVideo.User || apiVideo.user
  }
} 