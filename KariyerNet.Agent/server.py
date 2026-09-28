import os
import json
import traceback
import grpc
from concurrent import futures
from dotenv import load_dotenv
from google import genai
from google.genai import types
import time

import agent_pb2
import agent_pb2_grpc

load_dotenv()
client = genai.Client(api_key=os.getenv("GEMINI_API_KEY"))
MODEL_NAME = "gemini-3.6-flash"


def ask_gemini(contents, max_retries: int = 3) -> dict:
    last_error = None
    for attempt in range(max_retries):
        try:
            response = client.models.generate_content(
                model=MODEL_NAME,
                contents=contents,
                config=types.GenerateContentConfig(
                    response_mime_type="application/json"
                )
            )
            return json.loads(response.text)
        except Exception as e:
            last_error = e
            print(f"Deneme {attempt + 1}/{max_retries} başarısız: {e}")
            if attempt < max_retries - 1:
                time.sleep(3)
    raise last_error
class AgentServiceServicer(agent_pb2_grpc.AgentServiceServicer):

    def AnalyzeCv(self, request, context):
        try:
            cv_part = types.Part.from_bytes(data=request.cv_file, mime_type=request.mime_type)
            instruction = """Bu belge bir CV'dir. Sadece JSON formatında cevap ver, başka hiçbir açıklama ekleme.


Şu formatta cevap ver:
{{
  "skills": ["beceri1", "beceri2", ...],
  "experience_level": "Junior" veya "Mid" veya "Senior",
  "summary": "1-2 cümlelik özet",
  "suggestions": ["CV'yi güçlendirmek için somut, uygulanabilir 2-4 öneri, örneğin eksik bir bölüm, zayıf bir ifade, ölçülebilir başarı eksikliği gibi konularda"]
}}"""
            data = ask_gemini([instruction,cv_part])
            return agent_pb2.AnalyzeCvResponse(
            skills=data.get("skills", []),
            experience_level=data.get("experience_level", ""),
            summary=data.get("summary", ""),
            suggestions=data.get("suggestions", [])
        )
        except Exception as e:
            print("=== HATA (AnalyzeCv) ===")
            traceback.print_exc()
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(str(e))
            return agent_pb2.AnalyzeCvResponse()

    def MatchCandidateToJob(self, request, context):
        try:
            cv_part = types.Part.from_bytes(data=request.cv_file, mime_type=request.mime_type)
            instruction = f"""Bu belge bir adayın CV'sidir. Bu CV'yi aşağıdaki iş ilanıyla karşılaştır. Sadece JSON formatında cevap ver, başka hiçbir açıklama ekleme.



İlan Başlığı: {request.job_title}
İlan Açıklaması: {request.job_description}
İlan Gereksinimleri: {request.job_requirements}

İKİ AYRI açıklama üret:
1. "explanation": İşverene hitaben, üçüncü şahıs dille ("bu aday", "adayın CV'sinde" gibi).
2. "candidate_explanation": Doğrudan adayın kendisine hitaben, ikinci şahıs dille ("CV'nizde", "sahip olduğunuz" gibi).

Şu formatta cevap ver:
{{
  "compatibility_score": 0 ile 100 arasında bir sayı,
  "explanation": "işverene hitaben 2-3 cümle",
  "candidate_explanation": "adaya hitaben 2-3 cümle",
  "missing_skills": ["ilanda istenip CV'de olmayan beceriler"]
}}"""
            data = ask_gemini([instruction,cv_part])
            return agent_pb2.MatchResponse(              
            compatibility_score=data.get("compatibility_score", 0),
            explanation=data.get("explanation", ""),
            candidate_explanation=data.get("candidate_explanation", ""),
            matching_skills=data.get("matching_skills", []),
            missing_skills=data.get("missing_skills", [])
            )
        except Exception as e:
            print("=== HATA (MatchCandidateToJob) ===")
            traceback.print_exc()
            context.set_code(grpc.StatusCode.INTERNAL)
            context.set_details(str(e))
            return agent_pb2.MatchResponse()


def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    agent_pb2_grpc.add_AgentServiceServicer_to_server(AgentServiceServicer(), server)
    server.add_insecure_port('[::]:50051')
    server.start()
    print("Agent gRPC server 50051 portunda çalışıyor (Gemini ile)...")
    server.wait_for_termination()


if __name__ == '__main__':
    serve()