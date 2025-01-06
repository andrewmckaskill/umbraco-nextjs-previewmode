import type { NextApiRequest, NextApiResponse } from "next";
import crypto from "node:crypto";

export default async function preview(
  req: NextApiRequest,
  res: NextApiResponse
) {
  if (
    req.query.path === undefined ||
    req.query.expiry === undefined ||
    req.query.sig === undefined
  ) {
    return res.status(401).json({ message: "Invalid request" });
  }

  // check that preview secret has been configured
  const secretAsString = process.env.UMBRACO_PREVIEW_SECRET;
  if (secretAsString === null || secretAsString === undefined) {
    return res.status(400).json({ message: "Preview not configured" });
  }

  // validate preview signature
  const message = `${req.query.path}|${req.query.expiry}`;
  const secretAsBytes = Buffer.from(secretAsString, "utf8");

  const newSig = crypto
    .createHmac("sha256", secretAsBytes)
    .update(Buffer.from(message, "utf8"))
    .digest("base64url");

  if (newSig !== req.query.sig) {
    return res.status(401).json({ message: "Unauthorized request" });
  }

  // only allow relative paths to avoid open redirects
  const path = req.query.path as string;
  if (!path.startsWith("/")) {
    return res.status(401).json({ message: "Invalid request" });
  }

  // validate expirytime
  const expiryTime = parseInt(req.query.expiry as string);
  if (Number.isNaN(expiryTime)) {
    return res.status(401).json({ message: "Invalid request" });
  }

  const currentTime = Math.floor(new Date().getTime() / 1000);
  if (currentTime > expiryTime) {
    return res.status(401).json({ message: "Preview link expired" });
  }

  res.setDraftMode({ enable: true });

  console.debug(`[Preview] Redirecting to: ${path}`);

  return res.redirect(path);
}
